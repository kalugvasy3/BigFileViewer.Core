namespace FsharpXAMLLibrarySupport

open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
open System.Collections.Generic

open  System.Windows
open  System.Windows.Shapes
open  System.Windows.Controls
open  System.Windows.Input
open  System.Windows.Media
open  System.Windows.Media.Imaging
open  System.Windows.Threading

open System.IO
open System.Windows.Markup
open System.Xaml
open System.Reflection
open System.Windows.Media

open CommonClassLibrary
open FsharpXAMLLibrary 
open Utilities

open Microsoft.FSharp.Control
open System.Text
open System.Threading

type private MyTextBlock() as this = 
     inherit UserControl()

     let mutable position : int*int = (0,0) // line * position in line

     let mutable txtBlock = new TextBlock()
     do this.Content <- txtBlock      
   
     do txtBlock.Height <- 22.0


     member x.Text with set(v) = txtBlock.Text <- v   
     member x.Position with get() = position and set(v) = position <- v 
     
     

type QuickFind()  as this = 
    inherit  UserControl()
    // Load CaretTxt.xaml

    do this.Content <- contentAsXamlObjectFromAssembly("FsharpXAMLLibrarySupport","QuickFind")  // Load XAML
 
    let mutable myTextBox = ref ( new MyTextBox())
    let mutable openUpdateMMF = ref ( new OpenUpdateMMF() )
    let mutable refListTestbAll = ref (new  List<StringBuilder>())
    
    let mutable txtQuickFind : TextBox = this.Content?txtQuickFind 
    let mutable btnFindAll : Button = this.Content?btnFindAll
    let mutable btnFindNext : Button = this.Content?btnFindNext
    let mutable btnStop : Button = this.Content?btnStop
    let mutable stackPanel : StackPanel = this.Content?stackPanel  
    let mutable userClkl : UserClock = this.Content?userClk
    let mutable progressBar : ProgressBar = this.Content?progressBar

    let typeOfFind = new Event<bool>() 

    let findInText (x : int*int) = let (iLine, iChar) = x
                                   do myTextBox.Value.IntFirstLineOnPage <- iLine  //Screen will be updated automaticaly
                                   do myTextBox.Value.IntFirstCharOnPage <- iChar

    let addNewPosition(x: int*int , str: string) = 
                                   let mutable txt = new MyTextBlock()
                                   txt.Position <- x
                                   txt.Text <- x.ToString() + "  " + str
                                   txt.MouseLeftButtonDown.Add(fun _ -> findInText(txt.Position)) 
                                   stackPanel.Children.Add(txt)


    do progressBar.Minimum <- 0.0
    do progressBar.Maximum <- 100.0

    let progressBar(iBlock : int) = 
                   this.Dispatcher.Invoke(new Action(fun () -> 
                           if openUpdateMMF.Value.LongNumberOfBlocks <> 0L then
                               let fcurent = ((iBlock + 1) * 100) / (int)openUpdateMMF.Value.LongNumberOfBlocks
                               if (fcurent % 5 = 0) then  do progressBar.Value <- (float fcurent)
                                                          do Thread.Sleep(200) 
                               if fcurent = 100 then do progressBar.Value <- 0.0   
                                                        Thread.Sleep(200) ))

    let mutable prevFindBlock = -1
    let mutable blnStopSearch = false
 
    let loadAndSearch(iBlock, str : string, blnAll : bool) =      
             let mutable intStartLine : int = 0
             let mutable intStartChar : int = openUpdateMMF.Value.IntFirstCharOnPage
                 
             match  iBlock - (int)openUpdateMMF.Value.LongCurrentBlock with
             | -1 -> do refListTestbAll  <- openUpdateMMF.Value.RefListPreviousSbAll
                     do prevFindBlock <- (int)openUpdateMMF.Value.LongCurrentBlock  - 1
                     do intStartLine <- openUpdateMMF.Value.IntFirstLineOnPage - openUpdateMMF.Value.FirstLine(prevFindBlock) 

             |  0 -> do refListTestbAll  <- openUpdateMMF.Value.RefListCurrentSbAll
                     do prevFindBlock <- (int)openUpdateMMF.Value.LongCurrentBlock 
                     do intStartLine <- openUpdateMMF.Value.IntFirstLineOnPage - openUpdateMMF.Value.FirstLine(prevFindBlock) 
                

             |  1 -> do refListTestbAll  <- openUpdateMMF.Value.RefListNextSbAll
                     do prevFindBlock <- (int)openUpdateMMF.Value.LongCurrentBlock + 1
                     do intStartLine <- openUpdateMMF.Value.IntFirstLineOnPage - openUpdateMMF.Value.FirstLine(prevFindBlock) 

             |  _ -> if prevFindBlock <> iBlock then
                        do openUpdateMMF.Value.GetContentFromMMF (refListTestbAll , iBlock  , true, false , "F") 
                        do prevFindBlock <- iBlock  
                        do intStartLine <- 0
         
             do Thread.Sleep(0)

             if intStartLine < 0 then intStartLine <- 0
             let linesInBlock : int = refListTestbAll.Value.Count

             

             let find() = let mutable iy = -1
                          let mutable ix = -1
                          let mutable partOfString = ""
                          let mutable blnContinue = true
                          let listResult = new List<int*int*string>()

                          for iL = intStartLine to linesInBlock - 1 do
                              if (blnContinue || blnAll) && not blnStopSearch then
                                  let len =   refListTestbAll.Value.[iL].Length - intStartChar - str.Length 
                                  let mutable oneStr = "" 

                                  match len < 0 with
                                  | true -> oneStr <- ""
                                  | false -> oneStr <- refListTestbAll.Value.[iL].ToString(intStartChar + str.Length, len) 

                                  if  oneStr.Contains(str) 
                                      then
                                          ix <- refListTestbAll.Value.[iL].ToString().IndexOf(str, intStartChar + str.Length)
                                          iy <- iL + openUpdateMMF.Value.FirstLine(iBlock)
                                          //let ilen = Math.Min(50, refListTestbAll.Value.[iL].ToString().Length - ix - str.Length)
                                          partOfString <- refListTestbAll.Value.[iL].ToString().Substring(ix, str.Length)
                                          blnContinue <- false
                                          listResult.Add(iy , ix, partOfString)
                                      else intStartChar <- -str.Length
                          Thread.Sleep(0)
                          progressBar(iBlock)
                          listResult
             find()     

         



    let findNext(str : string) =    

         let mutable endBlock = (int)openUpdateMMF.Value.LongNumberOfBlocks - 1  // base on 0 block
         let rec loop n =           
             if n <= endBlock &&  str.Trim() <> "" && not blnStopSearch then
                 let lstResult = loadAndSearch(n, str, false)
                 if lstResult.Count = 0 
                     then loop (n + 1)
                     else
                         let (iy , ix, partStr) = lstResult.[0]
                         if iy >= 0 then progressBar(-1)
                                         (iy , ix) 
                                    else progressBar(n + 1)
                                         loop (n + 1)
             else  do progressBar(-1)
                   do blnStopSearch <- false
                   (-1 , -1)  
                   
         do blnStopSearch <- false

         let iCurrent = openUpdateMMF.Value.CalculateCurrentBlock ("R")
         let iEnd = openUpdateMMF.Value.ArrayOfBlockInfo.Length - 1

         if str.Length > 0 
             then 
                 loop(iCurrent)
             else (-1 , -1)

    
    let mutable blnFindAll = false


    let findNext() =
           do this.Dispatcher.Invoke(new Action(fun () -> do userClkl.Visibility <- Visibility.Visible))
           do myTextBox.Value.TextSearch <- txtQuickFind.Text.Trim()
           do typeOfFind.Trigger(false)
           do blnFindAll <- false
           let (iLine, iChar) = findNext(txtQuickFind.Text.Trim())
           do myTextBox.Value.IntFirstLineOnPage <- iLine
           do myTextBox.Value.IntFirstCharOnPage <- iChar
           do this.Dispatcher.Invoke(new Action(fun () -> do userClkl.Visibility <- Visibility.Collapsed ))
                           


    
    let findAll() =   
            typeOfFind.Trigger(true) 
            //let mutable endBlock = (int)openUpdateMMF.Value.LongNumberOfBlocks - 1  // base on 0 block
            
            //let rec loop n =           
            //    if n <= endBlock &&  str.Trim() <> "" && not blnStopSearch then
            //        let (iy , ix, partStr) = loadAndSearch(n, str)
            //        if iy >= 0 then progressBar(-1)
            //                        (iy , ix) 
            //                   else progressBar(n + 1)
            //                        loop (n + 1)
            //    else  do progressBar(-1)
            //          do blnStopSearch <- false
            //          (-1 , -1)  
          
            //do blnStopSearch <- false
            //if str.Length > 0 
            //    then 
            //         loop(openUpdateMMF.Value.CalculateCurrentBlock ("R")) //"R" ignore 
            //    else (-1 , -1)








    do txtQuickFind.TextChanged.Add(fun _ -> stackPanel.Children.Clear() )     
    do this.Dispatcher.Invoke(new Action(fun () -> do userClkl.Visibility <- Visibility.Collapsed ))
    
    do btnFindAll.Click.Add(fun _ -> findAll())                               
    
    do btnFindNext.Click.Add(fun _ -> findNext()  )

    do btnStop.Click.Add(fun _ -> do blnStopSearch <- true) 
    
    [<CLIEvent>]
    member x.TypeOfFind =   typeOfFind.Publish    

    
    member x.InitMyTextBox(txt : MyTextBox ref ) =
                        do myTextBox <- txt
                        do openUpdateMMF <- txt.Value.OpenUpdateMMF
 
  
                                                            
  
 

    

    
 



