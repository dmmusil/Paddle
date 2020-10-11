using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Paddle.API
{
    public class TableStorageCheckpointRepository : ICheckpointRepository
    {
        private readonly CloudTableClient _table;
        private Checkpoint _checkpoint;
        public TableStorageCheckpointRepository(CloudTableClient table)
        {
            _table = table;
        }

        public async Task<long> GetCheckpoint()
        {
            var table = _table.GetTableReference("checkpoints");
            await table.CreateIfNotExistsAsync();
            var operation =
                TableOperation.Retrieve<Checkpoint>("main-reactor", "instance");
            var result = await table.ExecuteAsync(operation);
            long value = -1;
            if (result.HttpStatusCode == 200)
            {
                _checkpoint = (Checkpoint)result.Result;
                value = _checkpoint.Value;
            }
            else
            {
                _checkpoint = new Checkpoint { Value = -1 };
                await table.ExecuteAsync(TableOperation.Insert(_checkpoint));
            }

            return value;
        }

        public async Task UpdateCheckpoint(long value)
        {
            var table = _table.GetTableReference("checkpoints");
            await table.CreateIfNotExistsAsync();
            _checkpoint.Value = value;
            var operation = TableOperation.InsertOrReplace(_checkpoint);
            await table.ExecuteAsync(operation);
        }

        public void ClearCheckpoint()
        {
            Task.Run(async () =>
            {
                await GetCheckpoint();
                await UpdateCheckpoint(-1);
            }).Wait();
        }
    }
}