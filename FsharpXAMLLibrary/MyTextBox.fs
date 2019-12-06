namespace FsharpXAMLLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.IO
open System.Windows.Markup
open System.Windows.Input
open System.Reflection
open System.Threading.Tasks
open System.Threading

open CommonClassLibrary

open Utilities

open System.Windows.Threading
open System.Windows.Input
open System.Windows.Media


type MyTextBox() as this  = 
    inherit UserControl() 

    //do this.Content <- contentAsXamlObject("MyTextBox") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("FsharpXAMLLibrary","MyTextBox") // Load XAML

    
    let mutable userClock : UserClock = (this.Content)?userClock
    do userClock.Visibility <- Visibility.Collapsed 

    // FIND ALL OBJECTS IN THIS.CONTENT 
    let mutable openUpdateMMF = new OpenUpdateMMF()
    let mutable myFonts = new  CommonClassLibrary.Fonts()
    let mutable myTouch = new CommonClassLibrary.Touch()
    let mutable canvasMain : Canvas = (this.Content)?canvasMain 
    let mutable txtBlock : TextBlock = (this.Content)?txtBlock 
    let mutable lblDropHere : Label = (this.Content)?lblDropHere  
    let mutable scrollX : ScrollBar = (this.Content)?scrollX
    let mutable scrollY : ScrollBar = (this.Content)?scrollY          

    let mutable myMenu : MyMenu = (this.Content)?myMenu
    do myMenu.HostUserControl <- this

    let mutable tbX : TextBlock = (this.Content)?tbX
    let mutable tbY : TextBlock = (this.Content)?tbY

    let mutable statusBar = ref (new StatusBarSystem())

    let eventSysInfoUpdate = new Event<float>() 
    let openFind = new Event<_>() 
  
    
    let unLoaded(e : RoutedEventArgs) = if openUpdateMMF.Mmf <> null then openUpdateMMF.Mmf.Dispose() 
                                        GC.Collect()
                                    
    let initXScroll() = 
            do openUpdateMMF.IntFirstCharOnPage <- (int)scrollX.Value
            do openUpdateMMF.IntHorizCountCharsOnPage <- (int)(canvasMain.ActualWidth * myFonts.CoeffFont_Widh / myFonts.Tb_FontSize)
            do openUpdateMMF.IntHorizCountCharsOnPage <- match openUpdateMMF.IntHorizCountCharsOnPage < 0 with | true -> 0 | false -> openUpdateMMF.IntHorizCountCharsOnPage
            do scrollX.Minimum <- 0.0
            do scrollX.Maximum <- match openUpdateMMF.IntMaxCharsInLine <= 0 with | true ->  0.0 | false -> float (openUpdateMMF.IntMaxCharsInLine - 1) // size of longest line
            do scrollX.SmallChange <- 1.0   
            do scrollX.LargeChange <- float (openUpdateMMF.IntHorizCountCharsOnPage / 2);
            do tbX.Text <- "X: " + openUpdateMMF.IntFirstCharOnPage.ToString("0,0") + " of " + ((int)scrollX.Maximum).ToString("0,0") 
            do Thread.Sleep(10)

            



    let initYScroll() = 
            do openUpdateMMF.IntFirstLineOnPage <- (int)scrollY.Value
            do openUpdateMMF.IntVertCountLinesOnPage <- (int)(canvasMain.ActualHeight * myFonts.CoeffFont_High / myFonts.Tb_FontSize);

            do openUpdateMMF.IntVertCountLinesOnPage <- match openUpdateMMF.IntVertCountLinesOnPage < 0 with | true -> 0 | false -> openUpdateMMF.IntVertCountLinesOnPage
            do openUpdateMMF.IntLastLineOnPage <- match (openUpdateMMF.IntFirstLineOnPage + openUpdateMMF.IntVertCountLinesOnPage) < openUpdateMMF.IntNumberOfTotalLinesEstimation  with
                                                  | true -> (openUpdateMMF.IntFirstLineOnPage + openUpdateMMF.IntVertCountLinesOnPage) 
                                                  | false -> openUpdateMMF.IntNumberOfTotalLinesEstimation
            do scrollY.Maximum <- float openUpdateMMF.IntNumberOfTotalLinesEstimation
            do scrollY.SmallChange <- 1.0   // one line
            do scrollY.LargeChange <- float (openUpdateMMF.IntVertCountLinesOnPage / 2)   // number lines per page/2
            do tbY.Text <- "Y: " + openUpdateMMF.IntFirstLineOnPage.ToString("0,0") + " of " + ((int)scrollY.Maximum).ToString("0,0") + "   (" + openUpdateMMF.IntVertCountLinesOnPage.ToString() + ")";     
            do Thread.Sleep(10)



    let mutable scale = 1.0
    let scalePlus() =  if scale < 1.75 then scale <- scale + 0.05
                       (!statusBar).ScaleNewValue <- scale
                       do eventSysInfoUpdate.Trigger(scale)



    let scaleMinus() = if scale > 0.75 then scale <- scale - 0.05
                       (!statusBar).ScaleNewValue <- scale
                       do eventSysInfoUpdate.Trigger(scale)



    let updateUserClock (blnVisible : bool) =

            this.Dispatcher.Invoke( fun _ ->   match blnVisible with
                                                | true ->  userClock.Visibility <- Visibility.Visible 
                                                           canvasMain.AllowDrop <- false         
                                                | false -> userClock.Visibility <- Visibility.Collapsed
                                                           canvasMain.AllowDrop <- true
                                                           do canvasMain.Focus() |> ignore )
            ignore()
    



    let update(blnChange : bool) =  
        [ async {
            this.Dispatcher.Invoke(new Action ( fun _ -> 
                                                   do initXScroll() 
                                                   if blnChange then do initYScroll()
                                                   ))

            this.Dispatcher.Invoke(new Action ( fun _ -> 
                                                   do openUpdateMMF.UpdateCurrentWindow(&txtBlock, blnChange, myMenu.TxtFind) |> ignore
                                                   if myMenu.TxtFind.Trim() = "" then do openUpdateMMF.BlnStopSearch <- true 
                                               )) 

                  } ] |> Async.Parallel |> Async.Ignore |> Async.Start                                  

                                                                          
 
    let updateDoubleY() = 
                         this.Dispatcher.Invoke(new Action ( fun _ ->
                                                   do openUpdateMMF.BlnStopSearch <- true
                                                   do updateUserClock(false) 
                                                   Thread.Sleep(100)
                                                   let deltaY = scrollX.ActualHeight
                                                   let curr = Mouse.GetPosition(scrollY)
                                                   let iy = int (scrollY.Maximum *  (curr.Y - deltaY) / (scrollY.ActualHeight - 2.0 * deltaY) )
                                                   Thread.Sleep(100)
                                                   scrollY.Value <- float iy 
                                                   )) 
                                                            

    let updateDoubleX() =
                         this.Dispatcher.Invoke(new Action ( fun _ ->
                                                   do openUpdateMMF.BlnStopSearch <- true
                                                   do updateUserClock(false) 
                                                   Thread.Sleep(100)

                                                   let deltaX = scrollY.ActualWidth
                                                   let curr = Mouse.GetPosition(scrollX)
                                                   let ix = int (scrollX.Maximum *  (curr.X - deltaX) / (scrollX.ActualWidth - 2.0 * deltaX))
                                                   Thread.Sleep(100)
                                                   scrollX.Value <- float ix 
                                                   )) 

 

    let wheel(e : MouseWheelEventArgs) = if e.Delta > 0 then scalePlus() 
                                                        else scaleMinus();




    let eventSysInfoStart(arg) = do this.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(fun _ ->
                                                 userClock.Visibility <- Visibility.Collapsed 
                                                 (!statusBar).PrgStatusValue <- arg 
                                                 (!statusBar).NumberTotalLines <- openUpdateMMF.IntNumberOfTotalLinesEstimation
                                                 do eventSysInfoUpdate.Trigger(scale)
                                    
                                                 match arg with
                                                 | 0.0 -> do canvasMain.AllowDrop <- true 
                                                          do updateUserClock(false)
                                                        //  do openUpdateMMF.BlnStopSearch <- true
                                                 | _ when arg > 0.0 -> do canvasMain.AllowDrop <- false
                                                 | _ when arg = -1.0 -> do MessageBox.Show("One or more line(s) exceed  4M ! \nUse Pro Version!") |> ignore
                                                                        do openUpdateMMF.PreInitFileOpen ()
                                                                        do Thread.Sleep(10)
                                                                        do update(true)
                                                                        do canvasMain.AllowDrop <- true  
                                                 | _ -> ignore()
                                                 
                                                 do  update(true)
                                               )) |> ignore
                              



    let scrolXWheel (e : MouseWheelEventArgs) =
            if e.Delta > 0 then scrollX.Value <- float(openUpdateMMF.IntFirstCharOnPage - 7)
                           else scrollX.Value <- float(openUpdateMMF.IntFirstCharOnPage + 7)




    let scrolYWheel (e : MouseWheelEventArgs) =
            if e.Delta > 0 then scrollY.Value <- float(openUpdateMMF.IntFirstLineOnPage - 3)
                           else scrollY.Value <- float(openUpdateMMF.IntFirstLineOnPage + 3)




    let canvasWheel (e : MouseWheelEventArgs) =
            match Keyboard.Modifiers with
            | ModifierKeys.Shift -> scrolXWheel(e)
            | ModifierKeys.Control -> wheel(e)
            | _ -> scrolYWheel(e)



    let canvasKeyDown (e : KeyEventArgs ) =
            match Keyboard.Modifiers with
            | ModifierKeys.Shift   -> match e.Key with
                                      | Key.Tab -> scrollX.Value <-  scrollX.Value - 4.0       // Right one Page
                                      | Key.PageUp -> scrollY.Value <- scrollY.Value - float openUpdateMMF.IntVertCountLinesOnPage * 100.0        // 100*Up
                                      | Key.PageDown -> scrollY.Value <- scrollY.Value + float openUpdateMMF.IntVertCountLinesOnPage * 100.0      // 100*Down
                                      | _ -> ignore()     // Left one Page

            | ModifierKeys.Control -> match e.Key with
                                      | Key.Home -> scrollX.Value <- 0.0
                                                    scrollY.Value <- 0.0
                                      | Key.End  -> scrollY.Value <- float openUpdateMMF.IntNumberOfTotalLinesEstimation - 1.0                   // Bottom
                                                    scrollX.Value <- 0.0
                                      | Key.PageUp   -> scrollY.Value <- scrollY.Value - float openUpdateMMF.IntVertCountLinesOnPage * 10.0        // 10*Up
                                      | Key.PageDown -> scrollY.Value <- scrollY.Value + float openUpdateMMF.IntVertCountLinesOnPage * 10.0      // 10*Down
                                      | Key.O        -> myMenu.Visibility <- Visibility.Visible
                                      | Key.F        -> openFind.Trigger()
                                      | _ -> ignore()

            | _ ->                    match e.Key with
                                      | Key.Escape -> openUpdateMMF.BlnStopSearch <- true
                                      | Key.PageUp -> scrollY.Value <- scrollY.Value - float openUpdateMMF.IntVertCountLinesOnPage        // Up
                                      | Key.PageDown -> scrollY.Value <- scrollY.Value + float openUpdateMMF.IntVertCountLinesOnPage      // Down
                                      | Key.Home -> scrollX.Value <- 0.0
                                      | Key.End -> scrollX.Value <-  float openUpdateMMF.IntMaxCharsInLine
                                      | Key.Left -> scrollX.Value <-  scrollX.Value - 1.0 
                                      | Key.Right -> scrollX.Value <-  scrollX.Value + 1.0 
                                      | Key.Up -> scrollY.Value <- scrollY.Value - 1.0
                                      | Key.Down -> scrollY.Value <- scrollY.Value + 1.0 
                                      | Key.Tab -> scrollX.Value <-  scrollX.Value + 4.0
                                      | _ -> ignore()



    let openFileTXT (files) =
            openUpdateMMF.Mmf <- null
            GC.Collect()
            
            // We MUST create  TaskScheduler and Synchronization Context
            let uiThread : TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();          

            // Show Clock - disable  AllowDrop
            do this.Dispatcher.Invoke(new Action ( fun () -> do updateUserClock (true)
                                                             do myMenu.Visibility <- Visibility.Collapsed
                                                             do scrollX.Value <-0.0
                                                             do scrollY.Value <-0.0
                                                             do canvasMain.AllowDrop <- false
                                       ))
            // Main Thread 
            let mainThreadLoadFile () =                       
                    let feedback = openUpdateMMF.CreateMMF(files)
                    
                    do update(true)
                    match feedback = "" with
                    | false-> do MessageBox.Show(feedback) |> ignore
                              do this.Dispatcher.Invoke(new Action ( fun () ->do canvasMain.AllowDrop <- true))
                    | true -> 
                              // Read Test Block
                              
                              do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListTestbAll , 0, true, false, "C") |> ignore  // Read Block 0 only (Current).
                              let x = openUpdateMMF.Investigate()
                              // LongOffset may changed by above function...
                              match x with
                              | _ when x = "" ->
                                         do openUpdateMMF.LongCurrentBlock  <- 0L
                                         do openUpdateMMF.IntMaxCharsInLine <- 0

                                         do openUpdateMMF.LongNumberOfBlocks <- (openUpdateMMF.LongTotalDocSize / openUpdateMMF.LongOfset) + 1L 

                                         do openUpdateMMF.InitArrayOfBlock() // <- it is Async Process 
                                         
                                         match openUpdateMMF.LongNumberOfBlocks with
                                          | 0L | 1L -> do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListCurrentSbAll, 0, true, false,"C") 

                                          | 2L -> do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListNextSbAll , 1, true, false,"N")
                                                  do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListCurrentSbAll, 0, true, true,"C")                                                 

                                          | _ ->  do openUpdateMMF.LongCurrentBlock  <- 1L
                                                  do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListCurrentSbAll, 1, true, false,"C") 
                                                  do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListNextSbAll , 2, true, true,"N")  
                                                  do openUpdateMMF.GetContentFromMMF(openUpdateMMF.RefListPreviousSbAll, 0, true, true, "P")   // Read Block 0 only (Current). 
                                        
                                         do this.Dispatcher.Invoke(new Action ( fun () -> do lblDropHere.Visibility <- Visibility.Collapsed
                                                                                          do (!statusBar).FullFileName <- openUpdateMMF.FullFileName
                                                                                          do (!statusBar).LongTotalDocSize <- openUpdateMMF.LongTotalDocSize 
                                                                                          do eventSysInfoUpdate.Trigger(scale)))

                              | x ->     do this.Dispatcher.Invoke(new Action ( fun () -> do lblDropHere.Visibility <- Visibility.Visible
                                                                                          do lblDropHere.Content <- x ))
                    true
            
            // Final Thread
            let finalThreadDoWOrk(x : bool) = 
                                    do this.Dispatcher.Invoke(new Action ( fun () -> 
                                                do eventSysInfoUpdate.Trigger(scale)
                                                do updateUserClock (false)
                                                do update(true)
                                                Thread.Sleep(200)
                                                do canvasMain.Focus() |> ignore))
            
            // Create Task                                
            let mainThreadDoWorkTask = Task<bool>.Factory.StartNew(fun () -> mainThreadLoadFile())            
            // Run Task - Impotent - each  task MUST return VALUE (if task does not return VALUE - it will be skipped)
            // It MUST be  Synchronization with Context
            do mainThreadDoWorkTask.ContinueWith( fun ( t : Task<bool> ) -> finalThreadDoWOrk(t.Result), uiThread ) |> ignore



    let openFileDrag (e: DragEventArgs) = 
            let data = e.Data
            let files = data.GetData(DataFormats.FileDrop) :?> string[] 
            openFileTXT(files)
            ()

    let mutable primaryX = 0.0
    let mutable primaryY = 0.0

    let mutable touchX = 0.0
    let mutable touchY = 0.0

 

    let exitApp() = let messageBoxText = "Do You want to exit from this Application ...?\n" // _Closing(object sender, System.ComponentModel.CancelEventArgs e)
                    let caption = "Exit"
                    let button = MessageBoxButton.YesNo
                    let icon = MessageBoxImage.Warning
                    let result = MessageBox.Show(messageBoxText, caption, button, icon) 
                    match result with
                    | MessageBoxResult.Yes -> Environment.Exit(0)
                    | _ -> ignore()                      // MessageBoxResult.No  -> e.Cancel = true;




    let goto(y ,x) = 
                    do this.Dispatcher.Invoke(new Action ( fun () -> 
                               match y < 0 , x < 0 with
                                  | true  , true  -> do MessageBox.Show("Not found beyond this point \nOr Cancelled [Esc] \nOr Jumped (duble ckick) to new position...") |> ignore                                            
                                  | false , true  -> scrollY.Value <- (float) y                                         
                                  | true  , false -> scrollX.Value <- (float) x                                                        
                                  | false , false -> scrollY.Value <- (float) y
                                                     scrollX.Value <- (float) x
                                                     do MessageBox.Show((y , x).ToString())  |> ignore  
                                  
                                  ))




    let nextSynchro(s : string[]) =
                        let uiThread : TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()          
                        let mainThreadDoWorkTask = Task<bool>.Factory.StartNew(fun () -> ( goto(openUpdateMMF.FindNext(s.[0]))
                                                                                           Thread.Sleep(100)     
                                                                                           true)) 
                        do mainThreadDoWorkTask.ContinueWith( fun ( t : Task<bool> ) -> ( 
                                                                                          do openUpdateMMF.BlnStopSearch <- true
                                                                                          do updateUserClock(false)
                                                                                         ), uiThread  ) |> ignore                                                                     
                        
                           

    do myMenu.EventMenu.Add(fun (c, s) ->  
                         match c with
                         | "Open" -> openFileTXT(s)
                                     myMenu.Visibility <- Visibility.Collapsed
                         | "Next" ->  nextSynchro(s : string[])
                         | "FindChanged" -> update(false)
                         | "Escape" -> openUpdateMMF.BlnStopSearch <- true
                         | "Exit" -> exitApp()
                         | _      -> ignore()
                            ) 
 


    do myMenu.EventGoTo.Add(fun (y,x) -> goto (y ,x))

       

    let mouseMove(e : MouseEventArgs) = 
            let curr = e.GetPosition
            ignore()



    // "UserControl" is loaded  -> set ALL events
    let setInit () =
            do txtBlock.FontFamily <- myFonts.Tb_FontFamily
            do txtBlock.FontSize <- myFonts.Tb_FontSize
            do txtBlock.TextEffects.Clear()

            do scrollX.ValueChanged.Add(fun e ->  update(false))
            do scrollY.ValueChanged.Add(fun e ->  update(true))  

            do scrollY.MouseDoubleClick.Add(fun _ -> updateDoubleY())
            do scrollX.MouseDoubleClick.Add(fun _ -> updateDoubleX())

            do scrollX.MouseWheel.Add(fun e -> scrolXWheel(e))
            do scrollY.MouseWheel.Add(fun e -> scrolYWheel(e))
 
            do canvasMain.MouseWheel.Add(fun e -> canvasWheel(e)) 
            do canvasMain.SizeChanged.Add(fun e -> update(true)) 
            do canvasMain.Drop.Add(fun e -> openFileDrag(e))
            do eventSysInfoUpdate.Trigger(scale)
            do canvasMain.Unloaded.Add(fun e -> unLoaded(e)) 

            do canvasMain.MouseMove.Add(fun e -> mouseMove(e))
            //do canvasMain.MouseLeftButtonDown.Add(fun e -> mouseLeftDown(e))
            //do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightDown(e))

            do canvasMain.KeyDown.Add(fun e -> canvasKeyDown (e))
            
            //do Touch.FrameReported.AddHandler( fun sender e -> myTouch.TouchFrame(sender, e))
            //do myTouch.MyCanvas <- canvasMain
            //do myTouch.EventTouch.Add(fun e -> moveResise(e)) 




    do openUpdateMMF.EventSysInfoStart.Add(fun arg -> eventSysInfoStart(arg))
    
    do this.Loaded.Add(fun e -> setInit ())
    do this.Unloaded.Add(fun e -> openUpdateMMF.PreInitFileOpen())

    do openUpdateMMF.EventBlockChanged.Add( fun (n , i) ->  
                         match (n , i) with
                         | (-1 , -1)              -> do update(true)
                                                     do updateUserClock (false)
                         | ( _ , -1) when n >= 0  -> do updateUserClock (true)
                         |   _                    -> do updateUserClock (true)
                                                     do this.Dispatcher.Invoke(new Action ( fun _ -> scrollY.Value <- float i ))

        )

    [<CLIEvent>]
    member x.EventSysInfoUpdate =  eventSysInfoUpdate.Publish
    [<CLIEvent>]
    member x.OpenFind = openFind.Publish

    member x.StatusBar with get() = statusBar and set(v)= statusBar <- v 

    member x.IntFirstCharOnPage with get() = (int)scrollX.Value and set(v) = scrollX.Value <- (double)v
    member x.IntFirstLineOnPage with get() = (int)scrollY.Value and set(v) = scrollY.Value <- (double)v

    member x.OpenUpdateMMF with get() = openUpdateMMF   










 
                                





