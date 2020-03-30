namespace CommonClassLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.IO
open System.Drawing
open System.Windows.Markup
open System.Windows.Input
open System.Reflection
open System.Threading.Tasks
open System.Threading

open System    
open System.Text
open System.Windows.Media
open System.Collections.Generic
open System.IO.MemoryMappedFiles
open System.Windows.Controls
open System.Threading

//open System.Data.SQLite

type OpenUpdateMMF() as _this   = 

    let eventSysInfoStart = new Event<float>() 
    let eventBlockChanged = new Event<int*int>() 
  
   
    let mutable intFirstLineOnPage = 0 
    let mutable intLastLineOnPage = 0 

    let mutable intFirstCharOnPage = 0 
    let mutable intLastCharOnPage = 0

    let mutable intHysteresis = 0
    
    let mutable intVertCountLinesOnPage = 0 
    let mutable intHorizCountCharsOnPage = 0 
    let mutable intMinLinesPerBloc = 0  // see init function 
    let mutable intLimitCharsPerLine = int  (2.0 ** 23.0)  // 4M  // 4M restriction...

    let mutable longCurrentBlock = 0L


    let mutable intMaxCharsInLine = 0 
    let mutable intNumberOfTotalLinesEstimation = 0

    let mutable refListTestbAll      = ref (new  List<StringBuilder>())
    let mutable refListPreviousSbAll = ref (new  List<StringBuilder>())
    let mutable refListCurrentSbAll  = ref (new  List<StringBuilder>())
    let mutable refListNextSbAll     = ref (new  List<StringBuilder>())

    let mutable blnPreviousRun       : bool ref = ref true
    let mutable blnCurrentRun        : bool ref = ref true
    let mutable blnThreadAllowRunnig : bool ref = ref true

    let mutable arrayPresentWindow : (StringBuilder * int) [] = [|(new StringBuilder() , 0)|]
    let mutable arrayOfBlockInfo : int[] = Array.empty
    

    //let mutable intCurrentBlockNumber = 0
    let mutable blnNextDone = false

    //File

    let mutable fullFileName  = ""
    let mutable longTotalDocSize = (int64)0
    let mutable longNumberOfBlocks = 0L

    let mutable mmf : MemoryMappedFile = null

    let mutable longOfset : int64 = 0L // see init function ...


    let mutable ucHolder : UserControl = new UserControl() 


    let mutable myFont = new CommonClassLibrary.Fonts()

    let mutable ARR : (StringBuilder * int) [] = [|(new StringBuilder() , 0)|] 

    let mutable allowSizeChange = true


    let firstLine(iBlock : int) =
            match iBlock with
            | 0 ->  0
            | _ when iBlock < 0 -> 0
            | _ when Array.isEmpty arrayOfBlockInfo -> 0
            | _ when iBlock >= (int)longNumberOfBlocks -> 
                    (arrayOfBlockInfo.[.. (int)longNumberOfBlocks - 1]  |> Array.fold(fun acc elem -> acc + elem) 0 ) + arrayOfBlockInfo.[(int)longNumberOfBlocks - 1] - 1
            | _  -> arrayOfBlockInfo.[..iBlock - 1]  |> Array.fold(fun acc elem -> acc + elem) 0 



    // Calculate   Current block by FirstLine on Page
    let calculateCurrentBlock (sReal) =  
            let mutable iC = 0
            if not (isNull arrayOfBlockInfo ) && arrayOfBlockInfo.Length > 0 then
                for i= 0 to arrayOfBlockInfo.Length - 1 do
                    if i < arrayOfBlockInfo.Length then
                        match  arrayOfBlockInfo.[i] > 0 with
                        | true ->  match intFirstLineOnPage > firstLine(i) with
                                   | true -> iC <- i
                                   | false -> ignore()
                        | false -> ignore()
            match sReal with
            | "R" -> ignore()
            | _ -> if iC = 0 then iC <- 1            
            iC                                   

                                                                            
    
    let mutable blnContinueContentFromMMF = false
    
    let rec getContentFromMMF (refListSbOrigin : List<StringBuilder> ref , intBlockNumber : int, blnAllLines : bool ,blnThread : bool, direction : string ) =    

            let mutable refListSb = ref (new List<StringBuilder>())
            let mutable offset = longOfset * int64(intBlockNumber)  // Begin from 0
            let mutable readInt64 = longOfset // Number Of Bytes for reading

            // Set Continue True ... During scrolling slide-bar it will be set False -> It allow cancel previous process.
            blnContinueContentFromMMF <- true

            // Apply restriction to readIn / offset
            match (offset < longTotalDocSize , offset + readInt64 < longTotalDocSize ) with
            | (true , true ) -> ignore()
            | (true , false) -> readInt64 <- longTotalDocSize - offset
            | (false, false) -> readInt64 <- 0L
            | ( _ , _ )      -> offset <- longTotalDocSize 
                                readInt64 <- (int64) 0     
            let mutable th : Thread = null 
            
            let threadContent() = 

                if not (isNull mmf) then  
                    use memoryMappedViewStream = mmf.CreateViewStream(offset, readInt64, MemoryMappedFileAccess.Read)
                    let mutable sr = new StreamReader(memoryMappedViewStream)
                    do refListSb.Value <- new List<StringBuilder>()                  
                     
                    match blnAllLines with
                    | true ->  // All Lines                           
                           
                            do eventBlockChanged.Trigger(intBlockNumber, -1)
                            Thread.Sleep(10)
                            
                            let mutable countLine = 0
                            while sr.Peek() >= 0 &&  (blnContinueContentFromMMF || direction = "I") do
                                  let mutable sb = new StringBuilder()
                                  do sb.Append(sr.ReadLine().Replace("\t", "    ")) |> ignore   // .Replace("�", "")
                                  //do sb.Append(sr.ReadLine()) |> ignore 
                                  let max = sb.Length
                                  if intMaxCharsInLine < max then intMaxCharsInLine <- max
                                  
                                  match countLine, intBlockNumber with
                                  | 0 , 0  -> do refListSb.Value.Add(sb)  // Do not combine first/last  // Add 0 line only for 0 block
                                  | 0 , _  -> ignore()
                                  | _ , _  -> do refListSb.Value.Add(sb)   
                                  do countLine <- countLine + 1 
                                  Thread.Sleep(0) // Must be Zero - it just point which can be used for interupr read ....
                            
                            Thread.Sleep(0)

                            // Last line in Block + First line in Next Block
                            if (blnContinueContentFromMMF || direction = "I") then 
                                  let mutable sbOneLine = ref (new  List<StringBuilder>())  
                                  do getContentFromMMF (sbOneLine , intBlockNumber + 1, false , false, direction) // Read one line from Next ...
                                
                                  match sbOneLine.Value.Count = 0 || refListSb.Value.Count =0  with
                                  | true -> ignore()
                                  | false ->  refListSb.Value.[refListSb.Value.Count - 1].Append(sbOneLine.Value.[0]) |> ignore
                                              let len = refListSb.Value.[refListSb.Value.Count - 1].Length 
                                              if intMaxCharsInLine < len then intMaxCharsInLine <- len

                    | false ->  // Just First Line
                            let mutable sb = new StringBuilder()
                            //do sb.Append(sr.ReadLine().Replace("�", "").Replace("\t", "     ")) |> ignore 
                            do sb.Append(sr.ReadLine()) |> ignore 
                            do refListSb.Value.Add(sb)
                            do intMaxCharsInLine <- match intMaxCharsInLine < sb.Length with  | true ->  sb.Length  | false -> intMaxCharsInLine  

                    if (blnContinueContentFromMMF || direction = "I" )  then refListSbOrigin  := refListSb.Value 
                    if direction <> "I" && blnAllLines && blnContinueContentFromMMF  then do eventBlockChanged.Trigger(-1, -1)
 
            
            match readInt64 with
            | 0L -> ignore()
            | _  -> match blnThread with 
                    | true -> do th <- new Thread(new ThreadStart(fun () -> threadContent())) 
                              do th.Start() 
                    | _    -> do threadContent()
            ()



    let preInitFileOpen () = 
            do arrayOfBlockInfo <- Array.empty                                    

            // We must give time for Dispose
            if not (isNull mmf)  then do mmf <- null
            GC.Collect()
            Thread.Sleep(100)   // 0.1 sec
            do arrayOfBlockInfo <- Array.empty
            do intFirstLineOnPage <- 0 
            do intLastLineOnPage <- 0 
            do intFirstCharOnPage <- 0 
    
            do intVertCountLinesOnPage <- 0 
            do intHorizCountCharsOnPage <- 0 
            do intMinLinesPerBloc <- 400  // Count Lines On Page should be less then Min Lines Per Block
            do longCurrentBlock <- 0L
            do intMaxCharsInLine <- 0 
            do intNumberOfTotalLinesEstimation <- 0
            do refListTestbAll  <- ref (new  List<StringBuilder>())
            do refListPreviousSbAll  <- ref (new  List<StringBuilder>())
            do refListCurrentSbAll  <- ref (new  List<StringBuilder>())
            do refListNextSbAll  <- ref (new  List<StringBuilder>())
            do arrayPresentWindow  <- [|(new StringBuilder(), 0)|]
            do longTotalDocSize <- 0L
            do longNumberOfBlocks <- 0L
            do longOfset <- int64 (2.0 ** 24.0)  // 2.0 ** 27.0 - 134217728L  // preliminary read 128M  // DO NOT CHANGE !!!!




    let progressBar(iBlock : int) = 
                   if longNumberOfBlocks <> 0L then
                       let fcurent = ((iBlock + 1) * 100) / (int)longNumberOfBlocks
                       if (fcurent % 5 = 0) then  do eventSysInfoStart.Trigger(float fcurent)
                                                  do Thread.Sleep(200) 
                       if fcurent = 100 then do eventSysInfoStart.Trigger(0.0)   
                                                Thread.Sleep(200)

    //let db = new SQLiteAccess()
    //let dbOpen() =            
    //        db.ConnectionString <- @"Data Source=:memory:"  //       Data Source=TMP.apv   \\Data Source=:memory:
    //        db.OpenDBConnection() |> ignore
    //        db.SbSqlCommandText <- new StringBuilder(@"DROP TABLE IF EXISTS [MYTMP]")
    //        db.ExecuteNonQuery(false) |> ignore       
    //        db.SbSqlCommandText <- new StringBuilder(@"CREATE TABLE [MYTMP] (ROWID INTEGER PRIMARY KEY AUTOINCREMENT, STR TEXT)")
    //        db.ExecuteNonQuery(false) |> ignore



    let initArrayOfBlock() =
        async {
            //dbOpen()

            do intNumberOfTotalLinesEstimation <- 0
            do longNumberOfBlocks <- (longTotalDocSize / longOfset) + 1L       
            do arrayOfBlockInfo <- Array.create (int longNumberOfBlocks) 0   
            do intMaxCharsInLine <- 0

            let blockRead(intBlockNumber : int) =  
                    let initRef = ref (new  List<StringBuilder>())
                    do getContentFromMMF (initRef , intBlockNumber, true , false, "I")                   
                    let countLine = initRef.Value.Count  
                    
                    //let mutable sb = new StringBuilder("INSERT INTO [MYTMP] (STR) " + System.Environment.NewLine)
                    //let mutable countSelect = 0
                    //for i=0 to countLine - 1 do
                    //    sb.Append(@"SELECT '").Append(initRef.Value.[i].Replace("'","''")).Append(@"'").Append(System.Environment.NewLine) |> ignore
                    //    countSelect <- countSelect + 1

                    //    match countSelect = 500 with
                    //    | false -> ignore()
                    //    | _ -> db.ExecuteNonQuery(sb, false) |> ignore
                    //           countSelect <- 0
                    //           sb <- new StringBuilder("INSERT INTO [MYTMP] (STR) " + System.Environment.NewLine)
                    //           ignore()

                    //    match (i = countLine - 1) || (countSelect = 0)  with
                    //    | false -> sb.Append(" UNION ").Append(System.Environment.NewLine) |> ignore
                    //    | true -> ignore()                              

                    //let mutable numEffect = 0
                    //if countSelect > 0 then db.ExecuteNonQuery(sb, false) |> ignore
                    //                        do numEffect <- db.IntNumberOfRow

                    match intMinLinesPerBloc > countLine with
                    | true -> do intMinLinesPerBloc <- countLine
                    | false -> ignore()
                    countLine

            let mutable iTotal = 0

            do arrayOfBlockInfo |> Array.iteri(fun i x ->            
                   let count = blockRead(i)
                   do arrayOfBlockInfo.[i] <- count
                   do iTotal <- iTotal + 1
                   do progressBar(iTotal)  
                   do intNumberOfTotalLinesEstimation <- intNumberOfTotalLinesEstimation  + count 
                 )

            if intNumberOfTotalLinesEstimation > 0 && intMaxCharsInLine < intLimitCharsPerLine 
                   then  do eventSysInfoStart.Trigger(0.0)
                   else  do preInitFileOpen ()
                         do eventSysInfoStart.Trigger(-1.0) 
 
              } |>   Async.Start



    let initCurrentWindowArray(i : int) =   // i - number of line         

            if arrayOfBlockInfo.Length > 0 && arrayOfBlockInfo.Length > int longCurrentBlock   
            then
                 let currentBlockFirstLine : int = firstLine(int longCurrentBlock)  // length 
                 let curentI : int = i + intFirstLineOnPage - currentBlockFirstLine 
             
                 let previousBlockFirsLine : int =  firstLine(int longCurrentBlock - 1)
                 let nextBlockFirstLine : int = firstLine(int longCurrentBlock + 1)
                 Thread.Sleep(0)

                 match curentI < 0 with
                 | true ->  let prevI = currentBlockFirstLine + curentI - previousBlockFirsLine   // currentBlockFirstLine - 1 is previous
                            if (arrayOfBlockInfo.[int longCurrentBlock - 1] / 2  > prevI) then do intHysteresis <- -1
                            if prevI >= 0 && prevI < refListPreviousSbAll.Value.Count 
                                then ((refListPreviousSbAll.Value).[prevI], (refListPreviousSbAll.Value).[prevI].Length) 
                                else (new StringBuilder(),0)               
                                     
             
                 | false -> match (curentI < nextBlockFirstLine - currentBlockFirstLine,  arrayOfBlockInfo.Length ) with
                            |  true, _  -> if  curentI < refListCurrentSbAll.Value.Count && curentI >=0
                                           then ((refListCurrentSbAll.Value).[curentI], (refListCurrentSbAll.Value).[curentI].Length)
                                           else  (new StringBuilder(),0)
             
                            |  false, 1 -> let nextI = curentI + currentBlockFirstLine + 1
                                           do intHysteresis <- 0
                                           if nextI - 1  < refListNextSbAll.Value.Count &&  nextI  > 0 // if have not loaded yet 
                                           then ((refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ], (refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ].Length)
                                           else (new StringBuilder(),0)

                            |  false, 2 -> let nextI = curentI + currentBlockFirstLine - nextBlockFirstLine + 1
                                           do intHysteresis <- 0
                                           if nextI - 1  < refListNextSbAll.Value.Count &&  nextI  > 0 // if have not loaded yet 
                                           then ((refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ], (refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ].Length)
                                           else (new StringBuilder(),0)

                            |  false, _ -> let nextI = curentI + currentBlockFirstLine - nextBlockFirstLine + 1
                                           if (nextI > arrayOfBlockInfo.[Math.Min(int longCurrentBlock + 1, arrayOfBlockInfo.Length - 1 )] / 2) then do intHysteresis <- +1
                                           if nextI - 1  < refListNextSbAll.Value.Count &&  nextI  > 0 // if have not loaded yet 
                                           then ((refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ], (refListNextSbAll.Value).[curentI + currentBlockFirstLine - nextBlockFirstLine ].Length)
                                           else (new StringBuilder(),0)

            else (new StringBuilder(),0)
 

    let updateArrayPresentWindow () =   
 
         let calcBlock =  calculateCurrentBlock ("") // base on firstLineOnPage                 
         let lines = if intLastLineOnPage - intFirstLineOnPage >= 0 then intLastLineOnPage - intFirstLineOnPage else 0
         
         let blnContinue() =
                 match  longCurrentBlock - 1L <= (int64) calcBlock  &&  (int64) calcBlock <= longCurrentBlock + 1L   with
                     | true ->  true

                     | false -> do longCurrentBlock <- (int64) calcBlock
                                
                                blnContinueContentFromMMF <- false
                                Thread.Sleep(0) //Very important - STOP ALL Thread - MUST BE "0"

                                do refListCurrentSbAll <- ref (new  List<StringBuilder>())
                                do refListPreviousSbAll <- ref (new  List<StringBuilder>())
                                do refListNextSbAll <- ref (new  List<StringBuilder>())


                                match ( longCurrentBlock > 0L , longCurrentBlock < longNumberOfBlocks - 1L) with
                                // ONE BLOCK ONLY
                                | (false, false) ->  do getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, true, "C")
                                                     //[async { getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, false, "C") }] |> Async.Parallel |> Async.Ignore |> Async.Start 
                                // TWO BLOCKS ONLY 
                                //| (true, false)  ->  
                                //          do getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, true,"C")
                                //          do getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true, true,"N")

                                //[async { getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, false,"C") };
                                //async { getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true, false,"N")} ] |> Async.Parallel |> Async.Ignore |> Async.Start 
                               
                                // THREE OR MORE                      
                                | (_ , _ )       ->  if longCurrentBlock = 0L then longCurrentBlock <- 1L
                                                     if longCurrentBlock = longNumberOfBlocks - 1L then longCurrentBlock <- longNumberOfBlocks - 2L 

                                                     do getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, true, "C")
                                                     do getContentFromMMF (refListPreviousSbAll , int longCurrentBlock - 1, true, true,"P")
                                                     do getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true, true,"N")
                                                     
                                                     //[async { getContentFromMMF (refListCurrentSbAll , int longCurrentBlock, true, false, "C") };                                                     
                                                     //async { getContentFromMMF (refListPreviousSbAll , int longCurrentBlock - 1, true, false,"P")}; 
                                                     //async { getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true, false,"N") }] |> Async.Parallel |> Async.Ignore |> Async.Start 
                                                 
                                false 

         do ARR <- Array.init lines (fun i -> initCurrentWindowArray(i))
         do allowSizeChange <- true
  
         do  match blnContinue() with
             | true ->  match intHysteresis with
                        | -1 -> if longOfset * (longCurrentBlock - 1L) > 0L then  
                               
                                   do refListNextSbAll <- refListCurrentSbAll
                                   do refListCurrentSbAll <- refListPreviousSbAll
                                   do refListPreviousSbAll <- ref (new  List<StringBuilder>())
                                   do longCurrentBlock <- longCurrentBlock - 1L 
                                   do getContentFromMMF (refListPreviousSbAll , int longCurrentBlock - 1, true,  true,"P")
                                   //[ async {getContentFromMMF (refListPreviousSbAll , int longCurrentBlock - 1, true,  false,"P")}] |> Async.Parallel |> Async.Ignore |> Async.Start     // do! - awaiting
                                   do intHysteresis <- 0     
    
                        | 0  -> ignore()
                 
                        | 1  -> if longOfset * (longCurrentBlock + 1L) < longTotalDocSize then  
                                                                                                                                                         
                                   do refListPreviousSbAll <- refListCurrentSbAll
                                   do refListCurrentSbAll <- refListNextSbAll
                                   do refListNextSbAll <- ref (new  List<StringBuilder>()) // Must create new "ref"                   
                                   do longCurrentBlock <- longCurrentBlock + 1L 
                                   do getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true,  true,"N")
                                   //[ async { getContentFromMMF (refListNextSbAll , int longCurrentBlock + 1, true,  false,"N") }] |> Async.Parallel |> Async.Ignore |> Async.Start     // do! - awaiting 
                                   do intHysteresis <- 0    
                        | _ -> ignore()
         
             | false -> ignore() // Do no change arr - not ready ...
         
         ARR

            



    //Update Current Window // We are working with Block 128M // Always CurrenBlock
    let updateCurrentWindow(txb : TextBlock ref , lenghtArr : List<int> ref , blnChange : bool, txtFind : string ) =        
            let textEffect (index, length) = 
                             let mutable scb = new SolidColorBrush(Colors.Magenta)
                             do scb.Freeze()
                             let te = new TextEffect(null, scb, null, index, length)
                             do (!txb).TextEffects.Add(te)
            let rec findAddTextEffect (index : int , s : string) =
                          let mutable localIndex = 0
                          if s.Contains(txtFind) && txtFind.Trim() <> "" then 
                             localIndex <- s.IndexOf(txtFind)
                             do textEffect(index + localIndex, txtFind.Length)
                             let ind = index + localIndex + txtFind.Length
                             do findAddTextEffect(ind, s.Substring(localIndex + txtFind.Length) )                           
            
            let mutable sbText = new StringBuilder()
            
            let intLastCharOnPage = intFirstCharOnPage + intHorizCountCharsOnPage
            let lines = arrayPresentWindow.Length
            let mutable index = 0
            do (!lenghtArr).Clear()

            let buildCurrent (i : int , v  : StringBuilder * int) =
                let (sb,iLen) = v 
                do (!lenghtArr).Add(iLen)
                
                if intFirstCharOnPage < iLen
                    then  let intTmp : int = match (intLastCharOnPage <= iLen) with | true -> intHorizCountCharsOnPage | false -> (iLen - intFirstCharOnPage)
                          let str : string = sb.ToString().Substring(intFirstCharOnPage, intTmp) + Environment.NewLine
                          do findAddTextEffect(index , str)
                          do index<- index + iLen 
                          do sbText.Append(str) |> ignore
                    else  do sbText.Append(Environment.NewLine) |> ignore               
                          do index <- index + Environment.NewLine.Length

            if blnChange then  do arrayPresentWindow <- updateArrayPresentWindow ()   
            do (!txb).TextEffects.Clear() 
            do arrayPresentWindow |> Array.iteri (fun i v -> buildCurrent (i , v) )   

            do (!txb).Text <- sbText.ToString()
            if (!lenghtArr).Count = 0 then (!lenghtArr).Add(0)
            true



    let createMMF (files : string[]) =
            do mmf <-null
            GC.Collect |>ignore
            Thread.Sleep(0)
            let mutable strReturn = "Please drop only ONE file!"          
            do preInitFileOpen ()
                      
            match files : string[] with
            | x when isNull x      -> ignore()
            | x when x.Length = 0 -> ignore()
            | x when x.Length > 1 -> ignore()

            | _ -> let fileTmp = new FileInfo(files.[0]) 
                   match fileTmp.Extension.ToUpper() with
                   | ".TXT" | ".SQL" | ".CSV" | ".XML" | ".XAML" |  ".LOG"  |  ".FB2"
                       -> try
                              do fullFileName <- fileTmp.FullName;           
                              do mmf <- MemoryMappedFile.CreateFromFile(fileTmp.FullName, FileMode.Open, fileTmp.Name);                           
                              do strReturn <- ""
                              do longTotalDocSize <- fileTmp.Length                    
                          with | ex -> do strReturn <- ex.Message

                   | _ -> do strReturn <- "Please Drop Only TXT|SQL|CSV|XML|XAML|LOG|FB2 file. "            
            strReturn        

 
 
    let investigate() =        
         let numLines = refListTestbAll.Value.Count 
         let minLines = intMinLinesPerBloc
         let r = float numLines / float minLines 
         do refListTestbAll  <- ref (new  List<StringBuilder>())
         let mutable strReturn = ""

         if longTotalDocSize >= 3L * longOfset 
            then
                 match r with
                 //| _ when r > 0.03125 && r <= 0.0625    -> longOfset <- longOfset * 32L;
                 //| _ when r > 0.0625 && r <= 0.125    -> longOfset <- longOfset * 16L;
                 //| _ when r > 0.125 && r <= 0.25    -> longOfset <- longOfset * 8L;
                 | _ when r <= 0.5    -> longOfset <- longOfset * 4L;  
                 | _ when r > 0.5  && r <= 1.0    -> longOfset <- longOfset * 2L;   
                 | _ when r > 1.0  && r <= 2.0    -> ignore();                     
                 | _ when r > 2.0  && r <= 4.0    -> longOfset <- longOfset / 2L;  
                 //| _ when r > 4.0  && r <= 8.0    -> longOfset <- longOfset / 4L;
                 //| _ when r > 8.0  && r <= 16.0   -> longOfset <- longOfset / 8L;
                 //| _ when r > 16.0  && r <= 32.0   -> longOfset <- longOfset / 16L;
                 //| _ when r > 32.0  && r <= 64.0   -> longOfset <- longOfset / 32L;
                 | _ when r > 4.0                -> longOfset <- longOfset / 4L; 
                 | _ ->  longOfset <- 0L
                         do strReturn <- "One or more line(s) exceed "+ intLimitCharsPerLine.ToString("0,0") + " chars, use Pro Version" 
            else longOfset <- 3L * longOfset 
            
         strReturn



    //let updateUcHolder() = 
    //        let h = ucHolder.ActualHeight
    //        do intVertCountLinesOnPage <- (int)(ucHolder.ActualHeight *  myFont.CoeffFont_High /  myFont.Tb_FontSize);
            
    //        match intVertCountLinesOnPage > intMinLinesPerBloc * 2 with   // it just means - block should include at least 2 screens ... 
    //        | true ->  ucHolder.MaxHeight <- ucHolder.ActualHeight / 2.0
    //                   ignore()
    //        | false -> ignore()


    let mutable prevFindBlock = -1
 
    let loadAndSearch(iBlock, str : string) =      
            let mutable intStartLine : int = 0
            let mutable intStartChar : int = intFirstCharOnPage
                    
            match  iBlock - (int)longCurrentBlock with
            | -1 -> do refListTestbAll  <- refListPreviousSbAll
                    do prevFindBlock <- (int)longCurrentBlock  - 1
                    do intStartLine <- intFirstLineOnPage - firstLine(prevFindBlock) 

            |  0 -> do refListTestbAll  <- refListCurrentSbAll
                    do prevFindBlock <- (int)longCurrentBlock 
                    do intStartLine <- intFirstLineOnPage - firstLine(prevFindBlock) 
                   

            |  1 -> do refListTestbAll  <- refListNextSbAll
                    do prevFindBlock <- (int)longCurrentBlock + 1
                    do intStartLine <- intFirstLineOnPage - firstLine(prevFindBlock) 

            |  _ -> if prevFindBlock <> iBlock then
                       do getContentFromMMF (refListTestbAll , iBlock  , true, false , "F") 
                       do prevFindBlock <- iBlock  
                       do intStartLine <- 0
            
            do Thread.Sleep(0)

            if intStartLine < 0 then intStartLine <- 0
            let linesInBlock : int = refListTestbAll.Value.Count

            let find() = let mutable iy = -1
                         let mutable ix = -1
                         let mutable blnContinue = true

                         for iL = intStartLine to linesInBlock - 1 do
                             if blnContinue then
                                 let len =   refListTestbAll.Value.[iL].Length - intStartChar - str.Length 
                                 let mutable oneStr = "" 

                                 match len < 0 with
                                 | true -> oneStr <- ""
                                 | false -> oneStr <- refListTestbAll.Value.[iL].ToString(intStartChar + str.Length, len) 

                                 if  oneStr.Contains(str) 
                                     then
                                         ix <- refListTestbAll.Value.[iL].ToString().IndexOf(str, intStartChar + str.Length)
                                         iy <- iL + firstLine(iBlock)
                                         blnContinue <- false
                                     else intStartChar <- -str.Length

                         let nextLastLine = firstLine(iBlock + 1) - 1                        
                         eventBlockChanged.Trigger(iBlock, nextLastLine)  // This repopulate ALL ref Blocks - see above
                         Thread.Sleep(100) 
                         (iy , ix) 
            
            find()     

            
    let mutable blnStopSearch = false


    let findNext(str : string) =        
        do blnStopSearch <- false
        let mutable endBlock = (int)longNumberOfBlocks - 1  // base on 0 block
        let rec loop n =           
            if n <= endBlock &&  str.Trim() <> "" && not blnStopSearch then
                let (iy , ix) = loadAndSearch(n, str)
                if iy >= 0 then progressBar(-1)
                                (iy , ix) 
                           else progressBar(n + 1)
                                loop (n + 1)
            else  do progressBar(-1)
                  do blnStopSearch <- false
                  (-1 , -1)                 
        if str.Length > 0 
            then 
                 loop(calculateCurrentBlock ("R"))  
            else (-1 , -1)


    let xmlToSqLite(strNameDB : string) =      
        let mutable intStartLine : int = 0
        let mutable intStartChar : int = 0
                
        let readBlock (iBlock : int) =
                do getContentFromMMF (refListTestbAll , iBlock  , true, false , "X") 
                do prevFindBlock <- iBlock  
                do intStartLine <- 0

        let xmlToSql() = 
                let mutable iy = -1
                let mutable ix = -1
                let mutable blnContinue = true

                for iL = intStartLine to refListTestbAll.Value.Count - 1 do ()
                    //if blnContinue then
                    //    let len =   refListTestbAll.Value.[iL].Length - intStartChar - str.Length 
                    //    let mutable oneStr = "" 

                    //    match len < 0 with
                    //    | true -> oneStr <- ""
                    //    | false -> oneStr <- refListTestbAll.Value.[iL].ToString(intStartChar + str.Length, len) 

                    //    if  oneStr.Contains(str) 
                    //        then
                    //            ix <- refListTestbAll.Value.[iL].ToString().IndexOf(str, intStartChar + str.Length)
                    //            iy <- iL + firstLine(iBlock)
                    //            blnContinue <- false
                    //        else intStartChar <- -str.Length
                                        //if blnContinue then
                    //    let len =   refListTestbAll.Value.[iL].Length - intStartChar - str.Length 
                    //    let mutable oneStr = "" 

                    //    match len < 0 with
                    //    | true -> oneStr <- ""
                    //    | false -> oneStr <- refListTestbAll.Value.[iL].ToString(intStartChar + str.Length, len) 

                    //    if  oneStr.Contains(str) 
                    //        then
                    //            ix <- refListTestbAll.Value.[iL].ToString().IndexOf(str, intStartChar + str.Length)
                    //            iy <- iL + firstLine(iBlock)
                    //            blnContinue <- false
                    //        else intStartChar <- -str.Length
        ignore() 
                    


    


    [<CLIEvent>]
    member x.EventSysInfoStart =  eventSysInfoStart.Publish
    [<CLIEvent>]
    member x.EventBlockChanged =  eventBlockChanged.Publish


    member x.IntFirstLineOnPage 
           with get()= intFirstLineOnPage and 
                set(v)=intFirstLineOnPage <-v
                       intLastLineOnPage <- intFirstLineOnPage + intVertCountLinesOnPage 

    member x.IntLastLineOnPage 
           with get()= intLastLineOnPage  

    member x.IntFirstCharOnPage  
           with get() = intFirstCharOnPage and 
                set(v)= intFirstCharOnPage <-v 
                        intLastCharOnPage <- intFirstCharOnPage + intHorizCountCharsOnPage

    member x.IntLastCharOnPage
           with get() = intLastCharOnPage 

    member x.IntVertCountLinesOnPage  
           with get()= intVertCountLinesOnPage and 
                set(v)=intVertCountLinesOnPage <-v 
                       intLastLineOnPage <- intFirstLineOnPage + intVertCountLinesOnPage 

    member x.IntHorizCountCharsOnPage  
           with get()= intHorizCountCharsOnPage and 
                set(v)=intHorizCountCharsOnPage <-v                        
                       intLastCharOnPage <- intFirstCharOnPage + intHorizCountCharsOnPage
   
    member x.LongCurrentBlock  
           with get()= longCurrentBlock and 
                set(v)=longCurrentBlock <-v         

    member x.IntMaxCharsInLine 
           with get()= intMaxCharsInLine and 
                set(v)=intMaxCharsInLine <-v 
    
    member x.IntNumberOfTotalLinesEstimation 
           with get()= intNumberOfTotalLinesEstimation and 
                set(v)=intNumberOfTotalLinesEstimation <-v 


    member x.RefListPreviousSbAll 
           with get()= refListPreviousSbAll and 
                set(v)=refListPreviousSbAll <-v 

    member x.RefListCurrentSbAll 
           with get()= refListCurrentSbAll and 
                set(v)=refListCurrentSbAll <-v 

    member x.RefListNextSbAll 
           with get()= refListNextSbAll and 
                set(v)=refListNextSbAll <-v 

    //member x.ResetRefListSb(r : List<StringBuilder> byref ) = r <- new  List<StringBuilder>()

    member x.BlnPreviousRun 
           with get()= blnPreviousRun and 
                set(v)=blnPreviousRun <-v 

    member x.BlnCurrentRun 
           with get()= blnCurrentRun and 
                set(v)=blnCurrentRun <-v 

    member x.BlnNextRun 
           with get()= blnThreadAllowRunnig and 
                set(v)=blnThreadAllowRunnig <-v 

    member x.ArrayPresentWindow  
           with get()= arrayPresentWindow and 
                set(v)=arrayPresentWindow <-v 

    member x.ArrayOfBlockInfo 
           with get()= arrayOfBlockInfo and 
                set(v)=arrayOfBlockInfo <-v  

    member x.BlnNextDone 
           with get()= blnNextDone and 
                set(v)=blnNextDone <-v 

    member x.FullFileName 
           with get()= fullFileName and 
                set(v)=fullFileName <-v 

    member x.LongTotalDocSize 
           with get()= longTotalDocSize and 
                set(v)=longTotalDocSize <-v 

    member x.LongNumberOfBlocks 
           with get()= longNumberOfBlocks and 
                set(v)=longNumberOfBlocks <-v 

    member x.Mmf  
           with get()= mmf and set(v)=mmf <-v 

    member x.LongOfset  
           with get()= longOfset and set(v)=longOfset <-v 
 

    member x.UpdateCurrentWindow(tb : TextBlock ref, lenghtArr : List<int> ref, blnChange : bool, txtFind : string) = updateCurrentWindow(tb , lenghtArr, blnChange, txtFind)
                                                                                
    member x.CreateMMF (files : string[]) = 
                       createMMF (files)

    member x.GetContentFromMMF (refListSb : List<StringBuilder> ref , intBlockNumber : int, blnAllLines : bool ,blnThread : bool, direction : string) =  
                               getContentFromMMF (refListSb, intBlockNumber, blnAllLines, blnThread, direction)

 // member x.BlnRunnigAll with get() = blnRunnigAll  and set(v) = blnRunnigAll <- v

    //member x.UcHolder 
    //       with get() = ucHolder and 
    //            set(v)= ucHolder <- v 
    //                    updateUcHolder()

    member x.IntMinLinesPerBloc  
           with get() = intMinLinesPerBloc and 
                set(v) = intMinLinesPerBloc <- v

    member x.InitArrayOfBlock() = initArrayOfBlock()

    member x.Investigate()  = investigate()

    member x.PreInitFileOpen () =  preInitFileOpen ()

    member x.MyFont 
           with get() = myFont and set(v) = myFont <- v

    member x.BlnContinueContentFromMMF 
           with get() = blnContinueContentFromMMF and 
                set(v) = blnContinueContentFromMMF<-v

    member x.FindNext(str : string) = findNext(str)

    member x.RefListTestbAll 
           with get() = refListTestbAll and 
                set(v)= refListTestbAll <- v

    member x.BlnStopSearch 
           with get() = blnStopSearch and 
                set(v) = blnStopSearch <- v


    member x.AllowSizeChange with get() = allowSizeChange



    ////https://stackoverflow.com/questions/5404267/streamreader-and-seeking            
    //let getActualPosition( reader : StreamReader) =  
    //        let flags : System.Reflection.BindingFlags = System.Reflection.BindingFlags.DeclaredOnly ||| System.Reflection.BindingFlags.NonPublic ||| System.Reflection.BindingFlags.Instance ||| System.Reflection.BindingFlags.GetField
    //        // The current buffer of decoded characters
    //        let charBuffer = reader.GetType().InvokeMember("charBuffer", flags, null, reader, null) :?> char[]
        
    //        // The index of the next char to be read from charBuffer
    //        let charPos = reader.GetType().InvokeMember("charPos", flags, null, reader, null) :?> int
        
    //        // The number of decoded chars presently used in charBuffer
    //        let charLen = reader.GetType().InvokeMember("charLen", flags, null, reader, null) :?> int 
        
    //        // The current buffer of read bytes (byteBuffer.Length = 1024; this is critical).
    //        let byteBuffer = reader.GetType().InvokeMember("byteBuffer", flags, null, reader, null) :?> byte[] 

    //        // The number of bytes read while advancing reader.BaseStream.Position to (re)fill charBuffer
    //        let byteLen = reader.GetType().InvokeMember("byteLen", flags, null, reader, null) :?> int 

    //        // The number of bytes the remaining chars use in the original encoding.
    //        let numBytesLeft = reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos) |> int64  

    //        // For variable-byte encodings, deal with partial chars at the end of the buffer
    //        let mutable numFragments = 0
    //        if byteLen > 0 && not reader.CurrentEncoding.IsSingleByte 
    //            then
    //                match reader.CurrentEncoding.CodePage with  
    //                // UTF-8
    //                | 65001 -> let mutable byteCountMask = byte(0)

    //                           while ((byteBuffer.[byteLen - numFragments - 1] >>> 6) = byte(2)) do // if the byte is "10xx xxxx", it's a continuation-byte
    //                               do numFragments <- numFragments + 1
    //                               do byteCountMask <- byteCountMask ||| (byte (1 <<< numFragments)) // count bytes & build the "complete char" mask

    //                           if (byteBuffer.[byteLen - numFragments - 1] >>> 6) = byte(3) then // if the byte is "11xx xxxx", it starts a multi-byte char.
    //                               do numFragments <- numFragments + 1
    //                               do byteCountMask <- byteCountMask ||| (byte)(1  <<< numFragments); // count bytes & build the "complete char" mask
    //                                                                                                  // see if we found as many bytes as the leading-byte says to expect
    //                           if  numFragments > 1 && (((byteBuffer.[byteLen - numFragments] >>> 7) - byte(numFragments)) = byteCountMask) then
    //                               do numFragments <- 0  // no partial-char in the byte-buffer to account for
    //                // UTF-16LE               
    //                | 1200 ->  if byteBuffer.[byteLen - 1] >= ( 0xd8 |> byte )  then  // high-surrogate
    //                              numFragments <- 2  // account for the partial character
    //                // UTF-16BE
    //                | 1201 ->  if byteBuffer.[byteLen - 2] >= ( 0xd8 |> byte ) then // high-surrogate
    //                              numFragments <- 2  // account for the partial character
    //                | _ -> ignore()
         
    //        reader.BaseStream.Position - numBytesLeft - int64(numFragments)


 

       

