namespace DataCommonFsharp

open System.Data
open System.Data.Common
open System.Text
open System

//internal - Used to specify that a member is visible inside an assembly but not outside it.

//Connection String - Example MS SQL - > Server=XXXXXX;Database=XXXXXXX;User Id=XXXXXXX; Password=XXXXXXX;

type internal DataCommon() =

    // Private section. See Members bellow...
    
    let mutable connDB : DbConnection = null 
    let mutable connectionString : string = ""
    let mutable providerName : string = ""
    let mutable errorString : string = ""
    let mutable sbSqlCommandText : StringBuilder = null
    let mutable numberOfRowAffected : int = -1 

    // Close private DB connection "connDB"
    
    let closeConnection() = match connDB with
                            | null -> ignore()
                            | x    -> do x.Close()
                                      do connDB <- null
    
    // Open private connection 
    
    let openDBConnection() =
        do errorString <- ""
        if connectionString <> "" && providerName <> ""  then
            try
                let factory = DbProviderFactories.GetFactory(providerName)
                do connDB <- factory.CreateConnection()
                do connDB.ConnectionString <- connectionString 
                do connDB.Open()
                true
            with
                | ex -> errorString <- ex.Message
                        false
        else errorString <- "Connection String AND/OR Provider Name are blank. "
             false

    // Execute None Query - see  numberOfRowAffected
                             
    let executeNonQuery() = 
        do errorString <- ""
        if openDBConnection() then
            try
                let mutable cmd = connDB.CreateCommand()
                do cmd.Connection <- connDB
                do cmd.CommandText <- sbSqlCommandText.ToString()
                do cmd.CommandType <- CommandType.Text
                do numberOfRowAffected <- cmd.ExecuteNonQuery()
                do closeConnection() 
                true 
            with 
                | ex -> do errorString <- ex.Message
                        do closeConnection() 
                        false
        else  do numberOfRowAffected <- -1
              false

    // Test DB Connection 

    let testConnection() =  if openDBConnection() then
                               closeConnection()
                               true
                            else false 
    // Get Data Set

    let getDataSet() = 
        if openDBConnection() then
            try 
                let factory = DbProviderFactories.GetFactory(providerName)
                let mutable ds: DataSet = new DataSet() 
                let mutable dbAdapter = factory.CreateDataAdapter()
                do dbAdapter.FillLoadOption <- LoadOption.PreserveChanges 
                 
                let cmd = connDB.CreateCommand()
                do cmd.CommandText <- sbSqlCommandText.ToString()
                do cmd.CommandType <- CommandType.Text // Procedure -> CALL procName (@param1,...,@paramN)
                
                do dbAdapter.SelectCommand <- cmd
                do numberOfRowAffected <- dbAdapter.Fill(ds)

                do closeConnection()
                ds
            with 
                | ex -> do errorString <- ex.Message
                        do closeConnection() 
                        null
        else  do numberOfRowAffected <- -1
              null
              
    // Get All Providers installed on your PC

    // Retrieve the installed providers and factories.
    //DataTable table = DbProviderFactories.GetFactoryClasses();

    //comboProviders.DisplayMemberPath = "Name";
    //comboProviders.SelectedValuePath = "InvariantName";
    //comboProviders.ItemsSource = table.DefaultView;
    //comboProviders.BeginInit();

                                             
    let getAllProviders () = let tbl = DbProviderFactories.GetFactoryClasses()
                             tbl


    // Get All OLE DB providers

    let getAllOldbProviders () =
                let mutable ds = new DataSet()
                do errorString <- ""
                let reader : System.Data.OleDb.OleDbDataReader  = System.Data.OleDb.OleDbEnumerator.GetEnumerator(Type.GetTypeFromProgID("MSDAENUM"))
                let mutable dt : DataTable = new DataTable()
                do dt.Load(reader)
                do dt.TableName <- "MSDAENUM"
                do ds.Tables.Add(dt)
                do ds.AcceptChanges()  // MUST Accept Changes
                ds
                                                                                                     
    // MEMBERS

    member x.ConnectionString  with get() = connectionString  and set(v) = connectionString <- v
    member x.ProviderName with get() = providerName and set(v) = providerName <-v

    member x.SbSqlCommandText  with get() = sbSqlCommandText  and set(v) = sbSqlCommandText <- v 

    member x.CloseConnection() = closeConnection()   // just for interrupt execution

    member x.TestConnection() = testConnection()
    member x.TestConnection(currentProviderName, currentConnectionString) = 
                            if isNull currentProviderName then providerName <- ""
                                else providerName <- currentProviderName.ToString()
                            do connectionString <- currentConnectionString
                            testConnection()

    member x.ExecuteNonQuery(currentProviderName, currentConnectionString, currentSbSQL) = 
                            do providerName <- currentProviderName 
                            do connectionString <- currentConnectionString
                            do sbSqlCommandText <- currentSbSQL
                            executeNonQuery()
    
    member x.ExecuteNonQuery(currentSbSQL) = 
                            do sbSqlCommandText <- currentSbSQL
                            executeNonQuery()
    
    member x.ExecuteNonQuery() = executeNonQuery() 


    member x.GetDataSet(currentProviderName, currentConnectionString, currentSbSQL) = 
                            if isNull currentProviderName then providerName <- ""
                                                          else providerName <- currentProviderName.ToString()
                            do connectionString <- currentConnectionString
                            do sbSqlCommandText <- currentSbSQL
                            getDataSet()
    
    member x.GetDataSet(currentSbSQL) = do sbSqlCommandText <- currentSbSQL
                                        getDataSet()

    member x.GetDataSet() = getDataSet()


    member x.IntNumberOfRow  with get() = numberOfRowAffected
    member x.StrError with get() = errorString and set(v) = errorString <- v

    member x.GetAllProviders()  = getAllProviders()
    member x.GetAllOldbProviders() = getAllOldbProviders()

