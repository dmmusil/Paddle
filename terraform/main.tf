provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "paddle_api_rg" {
  location = "eastus"
  name = "paddle-api-rg"
}

resource "azurerm_sql_server" "paddle_sql_server" {
  administrator_login = "dmusil"
  administrator_login_password = "4rT9!30dF"
  location = azurerm_resource_group.paddle_api_rg.location
  name = "paddle-sql"
  resource_group_name = azurerm_resource_group.paddle_api_rg.name
  version = "12.0"
}

resource "azurerm_mssql_database" "paddle_db" {
  name = "paddle-db"
  server_id = azurerm_sql_server.paddle_sql_server.id
  auto_pause_delay_in_minutes = 60
  sku_name = "GP_S_Gen5_1"
  min_capacity = 1
}

resource "azurerm_storage_account" "paddle_api_storage" {
  account_replication_type = "LRS"
  account_tier = "Standard"
  location = azurerm_resource_group.paddle_api_rg.location
  name = "paddleapistorage"
  resource_group_name = azurerm_resource_group.paddle_api_rg.name
}

resource "azurerm_signalr_service" "paddle_api_signalr" {
  location = azurerm_resource_group.paddle_api_rg.location
  name = "paddle-api-signalr"
  resource_group_name = azurerm_resource_group.paddle_api_rg.name
  sku {
    capacity = 1
    name = "Free_F1"
  }
}

resource "azurerm_app_service_plan" "paddle_api_asp" {
  location = azurerm_resource_group.paddle_api_rg.location
  name = "paddle-api-asp"
  resource_group_name = azurerm_resource_group.paddle_api_rg.name
  kind = "FunctionApp"
  sku {
    size = "Y1"
    tier = "Dynamic"
  }
}

resource "azurerm_function_app" "paddle_api" {
  app_service_plan_id = azurerm_app_service_plan.paddle_api_asp.id
  location = azurerm_resource_group.paddle_api_rg.location
  name = "paddle-api"
  resource_group_name = azurerm_resource_group.paddle_api_rg.name
  storage_account_name = azurerm_storage_account.paddle_api_storage.name
  storage_account_access_key = azurerm_storage_account.paddle_api_storage.primary_access_key
}
  
