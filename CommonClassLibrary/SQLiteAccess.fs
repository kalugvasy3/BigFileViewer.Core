

namespace CommonClassLibrary

open System.Data

open System.Data.SQLite

open System.Text
open System
open System.Runtime.InteropServices

//internal - Used to specify that a member is visible inside an assembly but not outside it.

//Connection String - Example MS SQL - > Server=XXXXXX;Database=XXXXXXX;User Id=XXXXXXX; Password=XXXXXXX;

type internal SQLiteAccess() =

    // Private section. See Members bellow...
    
    let mutable connDB : SQLite.SQLiteConnection = null
    let mutable connectionString : string = ""
    let mutable errorString : string = ""
    let mutable sbSqlCommandText : StringBuilder = null
    let mutable numberOfRowAffected : int = -1 

    // Close private DB connection "connDB"
    
    let mutable blnOpenDB = false   
    
    let closeConnection() = blnOpenDB <- false
                            match connDB with
                            | null -> ignore()
                            | x    -> do x.Close()
                                      do connDB <- null
    
    // Open private connection 


    let openDBConnection() =
        do errorString <- ""
        if connectionString <> ""  then
            try
                connDB <- new SQLite.SQLiteConnection()
                do connDB.ConnectionString <- connectionString 
                do connDB.Open()
                do blnOpenDB <- true
                true
            with
                | ex -> errorString <- ex.Message
                        blnOpenDB <- false
                        false
        else errorString <- "Connection String AND/OR Provider Name are blank. "
             blnOpenDB <- false
             false

    // Execute None Query - see  numberOfRowAffected
                             
    let executeNonQuery(blnClose : bool) = 
        do errorString <- ""
        if blnClose then  openDBConnection() |> ignore
        if blnOpenDB then
            try
                let mutable cmd = connDB.CreateCommand()
                do cmd.Connection <- connDB
                do cmd.CommandText <- sbSqlCommandText.ToString()
                do cmd.CommandType <- CommandType.Text
                do numberOfRowAffected <- cmd.ExecuteNonQuery()
                if blnClose then do closeConnection() 
                true 
            with 
                | ex -> do errorString <- ex.Message
                        if blnClose then do closeConnection()
                        false
        else  do numberOfRowAffected <- -1
              false


    let write() = let mutable cmd = connDB.CreateCommand()  
                  do cmd.Connection <- connDB
                 

    // Test DB Connection 

    let testConnection() =  if openDBConnection() then
                               closeConnection()
                               true
                            else false 
    // Get Data Set

    let getDataSet(blnClose : bool) = 
        if blnClose then  openDBConnection() |> ignore
        if blnOpenDB then
            try 
                let mutable ds: DataSet = new DataSet() 
                let mutable dbAdapter = new SQLite.SQLiteDataAdapter()
                do dbAdapter.FillLoadOption <- LoadOption.PreserveChanges 
                 
                let cmd = connDB.CreateCommand()
                
                do cmd.CommandText <- sbSqlCommandText.ToString()
                do cmd.CommandType <- CommandType.Text // Procedure -> CALL procName (@param1,...,@paramN)

                do dbAdapter.SelectCommand <- cmd
                do numberOfRowAffected <- dbAdapter.Fill(ds)

                if blnClose then do closeConnection() 
                ds
            with 
                | ex -> do errorString <- ex.Message
                        if blnClose then do closeConnection()
                        null
        else  do numberOfRowAffected <- -1
              null
              

    // https://github.com/Faithlife/System.Data.SQLite/blob/master/src/System.Data.SQLite/SQLiteConnection.cs
    //https://www.csharpcodi.com/vs2/?source=3221/CurlSharp/CurlSharp/NativeMethods.cs

    let BackupDatabase(destination : SQLiteConnection , destinationName : string , sourceName : string , pages : int , callback : SQLiteBackupCallback, retryMilliseconds : int ) =
                
                
                //VerifyNotDisposed(); 
                //if (m_connectionState != ConnectionState.Open) 
                //     throw new InvalidOperationException("Source database is not open."); 
                //if (destination == null) 
                //     throw new ArgumentNullException("destination"); 
                //if (destination.m_connectionState != ConnectionState.Open) 
                //     throw new ArgumentException("Destination database is not open.", "destination"); 
                //if (destinationName == null) 
                //     throw new ArgumentNullException("destinationName"); 
                //if (sourceName == null) 
                //     throw new ArgumentNullException("sourceName"); 
                //if (pages == 0) 
                //     throw new ArgumentException("pages must not be 0.", "pages");  
                
        //let backup = NativeInterop.sqlite3_backup_init
        ignore()



    member x.ConnectionString  with get() = connectionString  and set(v) = connectionString <- v

    member x.SbSqlCommandText  with get() = sbSqlCommandText  and set(v) = sbSqlCommandText <- v 

    member x.OpenDBConnection() = openDBConnection()
    member x.CloseConnection() = closeConnection()   // just for interrupt execution
    member x.BlnOpenDB with get() = blnOpenDB


    member x.TestConnection() = testConnection()
    member x.TestConnection(currentConnectionString) = 
                            do connectionString <- currentConnectionString
                            testConnection()

    member x.ExecuteNonQuery(currentConnectionString, currentSbSQL,blnClose) = 
                            do connectionString <- currentConnectionString
                            do sbSqlCommandText <- currentSbSQL
                            executeNonQuery(blnClose)
    
    member x.ExecuteNonQuery(currentSbSQL,blnClose) = 
                            do sbSqlCommandText <- currentSbSQL
                            executeNonQuery(blnClose)
    
    member x.ExecuteNonQuery(blnClose) = executeNonQuery(blnClose) 


    member x.GetDataSet(currentConnectionString, currentSbSQL, blnClose) = 
                            do connectionString <- currentConnectionString
                            do sbSqlCommandText <- currentSbSQL
                            getDataSet(blnClose)
    
    member x.GetDataSet(currentSbSQL, blnClose) = do sbSqlCommandText <- currentSbSQL
                                                  getDataSet(blnClose)

    member x.GetDataSet(blnClose) = getDataSet(blnClose)


    member x.IntNumberOfRow  with get() = numberOfRowAffected
    member x.StrError with get() = errorString and set(v) = errorString <- v


