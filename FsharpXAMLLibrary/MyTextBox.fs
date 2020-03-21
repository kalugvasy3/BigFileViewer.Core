namespace FsharpXAMLLibrary

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Controls.Primitives
open System.Collections.Generic;
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

open ModuleTextBox
open System.Text

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
    let mutable crt = new CommonClassLibrary.CaretCanvas()

    let mutable blnInsert = false

    let mutable canvasMain : Canvas = (this.Content)?canvasMain 
    let mutable canvasSelecting : Canvas = (this.Content)?canvasSelecting 
    let mutable canvasSelected : Canvas = (this.Content)?canvasSelected 
    let mutable txtBlock : TextBlock = (this.Content)?txtBlock 
    let mutable lblDropHere : Label = (this.Content)?lblDropHere  
    let mutable scrollX : ScrollBar = (this.Content)?scrollX
    let mutable scrollY : ScrollBar = (this.Content)?scrollY  

    let mutable pointMouseLeftButtonPressed: Point = new Point();
    let mutable intPressedPositionY = 0
    let mutable intPressedPositionX = 0

    let mutable intAbsolutSelectVertStart = 0
    let mutable intAbsolutSelectHorizStart = 0
 
    let mutable intAbsolutSelectVertCurrent = 0
    let mutable intAbsolutSelectHorizCurrent = 0

    let mutable blnUpdateReady = true

    do canvasMain.Children.Add(crt) |> ignore

    let mutable myMenu : MyMenu = (this.Content)?myMenu
    do myMenu.HostUserControl <- this


    let mutable tbX : TextBlock = (this.Content)?tbX
    let mutable tbY : TextBlock = (this.Content)?tbY
    let mutable tbXY : TextBlock = (this.Content)?tbXY


    let mutable statusBar = ref (new StatusBarSystem())
    let mutable lenghtArr = new List<int>()
    do lenghtArr.Add(0)

    let openFind = new Event<_>() 
    let scaleCurrnet = new Event<float>()
    
    let unLoaded(e : RoutedEventArgs) = if openUpdateMMF.Mmf <> null then openUpdateMMF.Mmf.Dispose() 
                                        GC.Collect()


 

    //Function "Set Caret" to Absolute Position   
    let set_Caret() =          

        let iChar = crt.AbsoluteNumCharCurrent - openUpdateMMF.IntFirstCharOnPage
        let iLine = crt.AbsoluteNumLineCurrent - openUpdateMMF.IntFirstLineOnPage      

        do crt.TranslateTransform.X <- myFonts.Tb_FontSize * (float)iChar / myFonts.CoeffFont_Widh - 1.0
        do crt.TranslateTransform.Y <- myFonts.Tb_FontSize * (float)iLine / myFonts.CoeffFont_High       
        do crt.BackGroundColorCarete <- new SolidColorBrush(Colors.Magenta)
             
        do crt.FloatCareteH <- myFonts.Tb_FontSize / myFonts.CoeffFont_High       
        do crt.FloatCareteW <- match blnInsert with
                               | true  -> myFonts.Tb_FontSize / myFonts.CoeffFont_Widh 
                               | false -> myFonts.Tb_FontSize / myFonts.CoeffFont_Widh / 3.0
              
              
        if crt.AbsoluteNumCharCurrent >= openUpdateMMF.IntFirstCharOnPage &&
           crt.AbsoluteNumCharCurrent <= openUpdateMMF.IntLastCharOnPage  &&
           crt.AbsoluteNumLineCurrent >= openUpdateMMF.IntFirstLineOnPage &&
           crt.AbsoluteNumLineCurrent < openUpdateMMF.IntLastLineOnPage        
        
        then crt.Visibility <- Visibility.Visible                     
        else crt.Visibility <- Visibility.Hidden                      


        if crt.AbsoluteNumCharCurrent >= openUpdateMMF.IntFirstCharOnPage - 1 &&
           crt.AbsoluteNumCharCurrent <= openUpdateMMF.IntLastCharOnPage + 1 &&
           crt.AbsoluteNumLineCurrent >= openUpdateMMF.IntFirstLineOnPage - 1 &&
           crt.AbsoluteNumLineCurrent <= openUpdateMMF.IntLastLineOnPage + 1       
        
        then  do Keyboard.Focus(crt) |> ignore       // focus caretCanvas (textbox) itself
              do crt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down)) |> ignore    // move focus down to caret inside caretCanvas                
              do crt.Focus() |> ignore
                    
        else do Keyboard.Focus(canvasMain) |> ignore
             do canvasMain.Focus() |> ignore 
 
        do Thread.Sleep(0)



// Mouse Section - Move. Set. Selected


    
    let mutable pointMouseLeftButtonPressed = Point()
    let mutable intPressedPositionY = 0
    let mutable intPressedPositionX = 0
    
    let mutable intAbsolutSelectVertStart = 0
    let mutable intAbsolutSelectHorizStart = 0

    let mutable intNextAbsolutSelectionLine = 0
    let mutable intNextAbsolutSelectionHorizont = 0



    let mutable blnPlaceHolder = false
    let mutable blnPenDeSelect = false
    let mutable blnRecSelect = false
    let mutable blnRecDeSelect = false


    // Create selectedRectangle

    let set_Rectangle(intLine : int ref, intBeginChar: int ref, intEndChar: int ref, myBrush : Brush) =

        let mutable rect = new SelectedRectangle();

        //if intBeginChar > intEndChar 
        //then
        //    let intTmp = intBeginChar.Value
        //    intBeginChar.Value <- intEndChar.Value
        //    intEndChar.Value <- intTmp

        //if ((intBeginChar.Value > openUpdateMMF.IntLastCharOnPage ) ||
        //    (intEndChar.Value < openUpdateMMF.IntFirstCharOnPage) ||
        //    (intLine.Value < openUpdateMMF.IntFirstLineOnPage) ||
        //    (intLine.Value >= openUpdateMMF.IntLastLineOnPage )) 
        //then
        //    rect.RecW <- 0.0
        //    rect.RecH <- 0.0
        //    rect.RecBrush <- new SolidColorBrush(Colors.Transparent)
        //    rect
        //else    

        //intBeginChar.Value <- if intBeginChar.Value > openUpdateMMF.IntFirstCharOnPage - 1 
        //                      then intBeginChar.Value
        //                      else openUpdateMMF.IntFirstCharOnPage

        //intEndChar.Value   <- if intEndChar.Value > openUpdateMMF.IntFirstCharOnPage + 1 
        //                      then intEndChar.Value
        //                      else openUpdateMMF.IntFirstCharOnPage

        let iBChar = intBeginChar.Value - openUpdateMMF.IntFirstCharOnPage
        let iEChar = intEndChar.Value - openUpdateMMF.IntFirstCharOnPage;

        let iLine = intLine.Value - openUpdateMMF.IntFirstLineOnPage

        rect.TrX <- myFonts.Tb_FontSize * (float)iBChar / myFonts.CoeffFont_Widh
        rect.TrY <- myFonts.Tb_FontSize * (float)iLine / myFonts.CoeffFont_High    

        rect.RecH <- myFonts.Tb_FontSize /  myFonts.CoeffFont_High
        if (iBChar - iEChar = 0) 
        then rect.RecW <- myFonts.Tb_FontSize / myFonts.CoeffFont_Widh / 3.0
        else rect.RecW <- (float)(iEChar - iBChar) * myFonts.Tb_FontSize / myFonts.CoeffFont_Widh
        rect

    let mapOfSelectedPosition = new MapOfSelectedPosition()


    let updateSelectingPosition() = ()


       

       //for i = intStartLine to intStopLine do 
       //    let (sb :StringBuilder, iLen : int) = openUpdateMMF.ArrayPresentWindow.[i - (int)scrollY.Value]
           
       //    if i = intStartLine && intStartLine <> intStopLine then

       //        if  intStartChar >= iLen then do intStartChar <- iLen
       //        do rect <- set_Rectangle(ref i ,ref intStartChar, ref iLen, myBrush);
       //        do mapOfSelectedPosition.Add(i, intStartChar, iLen) |> ignore

       //    if (i = intStartLine && intStartLine = intStopLine) then

       //        if (intStopChar >= iLen) then intStopChar <- iLen
       //        if (intStartChar >= iLen) then intStartChar <- iLen
       //        do rect <- set_Rectangle(ref i, ref intStartChar, ref intStopChar, myBrush)
       //        do mapOfSelectedPosition.Add(i, intStartChar, intStopChar) |> ignore

       //    if (i = intStopLine && intStartLine <> intStopLine) then
       //        if (intStopChar >= iLen) then intStopChar <- iLen
       //        do rect <- set_Rectangle(ref i, ref 0, ref intStopChar, myBrush);
       //        do mapOfSelectedPosition.Add(i, 0, intStopChar) |> ignore

       //    if (intStartLine < i && i < intStopLine) then
       //        do rect <- set_Rectangle(ref i, ref 0, ref iLen, myBrush);
       //        do mapOfSelectedPosition.Add(i, 0, iLen) |> ignore

       //    let mutable cc = new Canvas();

       //    this.Dispatcher.Invoke(new Action(fun () -> do c.Children.Add(rect) |> ignore
       //                                                do c.Children.Add(cc) |> ignore
       //                                                do c <- cc))



    let currentcurrentCanvasSelecting()   =       
            
            if blnUpdateReady then
                let colr = Colors.Indigo

                let mutable intStartLine = intAbsolutSelectVertStart       
                let mutable intStartChar = intAbsolutSelectHorizStart    

                let mutable intStopLine = intAbsolutSelectVertCurrent     
                let mutable intStopChar = intAbsolutSelectHorizCurrent 
            
                //let (start, lengthStart) = openUpdateMMF.ArrayPresentWindow.[intStartLine - (int)scrollY.Value]
                //let (stop, lengthStop) = openUpdateMMF.ArrayPresentWindow.[intStopLine - (int)scrollY.Value]
                // Re-arreange Star/Stop Line/Char

                //if (blnPlaceHolder)  // blnPlaceHolder - comes from Left Menu  - just select verical line
                //then
                //    if intStartLine >= openUpdateMMF.IntNumberOfTotalLinesEstimation then do intStartLine <- openUpdateMMF.IntNumberOfTotalLinesEstimation - 1
                //    if intStartChar >= lengthStart then do intStartChar <- lengthStart  
                //    if intStopLine >= openUpdateMMF.IntNumberOfTotalLinesEstimation then do intStopLine <- openUpdateMMF.IntNumberOfTotalLinesEstimation - 1
                //    if intStopChar >= lengthStop then do intStopChar <- lengthStop
            
                if (intStartLine > intStopLine)
                then
                    let mutable intTmp = intStartLine
                    intStartLine <- intStopLine
                    intStopLine <- intTmp

                    intTmp <- intStartChar
                    intStartChar <- intStopChar
                    intStopChar <- intTmp

                if (intStartLine = intStopLine && intStartChar > intStopChar) 
                then
                    let intTmp = intStartChar
                    intStartChar <- intStopChar
                    intStopChar <- intTmp
             
                if (blnPenDeSelect = false && blnRecSelect = false && blnRecDeSelect = false && blnPlaceHolder = false) then
                    let  clr = Colors.Indigo
                    let mutable rect = new SelectedRectangle()
                    do rect.Opacity <- 0.2;

                    let myBrush = new SolidColorBrush(clr)
                    //do myBrush.Freeze();

                    let mutable c = new Canvas()  // This approach increase performance in 4..6 times // do not need to invalidate Rec which we already added
                    do c <- canvasSelecting

                    do mapOfSelectedPosition.Empty()

                    for i = intStartLine to intStopLine do 
                        do Thread.Sleep(1)
                        if (i - openUpdateMMF.IntFirstLineOnPage) >= 0 
                        then 
                            let (sb :StringBuilder, iLen : int) = openUpdateMMF.ArrayPresentWindow.[i - openUpdateMMF.IntFirstLineOnPage]
                        
                            let iActualLeft = openUpdateMMF.IntFirstCharOnPage
                            let iActualRight = Math.Min(iLen,openUpdateMMF.IntLastCharOnPage)
                            let mutable iStartL = intStartLine
                            let mutable iStopL = intStopLine
                            let mutable iStartCh = intStartChar
                            let mutable iStopCh = intStopChar

                            if (intStopChar >= iActualRight) then iStopCh <- iActualRight
                            if (intStartChar >= iActualRight) then iStartCh <- iActualRight
                            if (intStopChar <= iActualLeft) then iStopCh <- iActualLeft
                            if (intStartChar <= iActualLeft) then iStartCh <- iActualLeft

                            if (i = intStartLine && intStartLine = intStopLine) then
                                do rect <- set_Rectangle(ref i, ref iStartCh, ref iStopCh, myBrush)
                                // Map must include absolute number
                                do mapOfSelectedPosition.Add(i, intStartChar, intStopChar) |> ignore


                            if i = intStartLine && intStartLine < intStopLine then                              
                                do rect <- set_Rectangle(ref i ,ref iStartCh, ref iActualRight, myBrush);
                                // Map must include absolute number
                                do mapOfSelectedPosition.Add(i, intStartChar, iLen) |> ignore

                            if (i = intStopLine && intStartLine < intStopLine) then
                                do rect <- set_Rectangle(ref i, ref iActualLeft, ref iStopCh, myBrush);
                                // Map must include absolute number
                                do mapOfSelectedPosition.Add(i, 0, intStopChar) |> ignore

                            if (intStartLine < i && i < intStopLine) then
                                do rect <- set_Rectangle(ref i, ref iActualLeft, ref iActualRight, myBrush);
                                // Map must include absolute number
                                do mapOfSelectedPosition.Add(i, 0, iLen) |> ignore

                            let mutable cc = new Canvas();
                            this.Dispatcher.Invoke(new Action(fun () -> do c.Children.Add(rect) |> ignore
                                                                        do c.Children.Add(cc) |> ignore
                                                                        do c <- cc))



    let mutable blnMouseLeftPressed = false


    let setMousePositionForMoving() =

        if Mouse.RightButton.ToString() <> "Pressed" 
           then   
                let p = Mouse.GetPosition(txtBlock) 
       
                do crt.AbsoluteNumLineCurrent <- openUpdateMMF.IntFirstLineOnPage + (int)(p.Y / myFonts.Tb_FontSize * myFonts.CoeffFont_High)
                do crt.AbsoluteNumCharCurrent <- openUpdateMMF.IntFirstCharOnPage + (int)((p.X +  myFonts.Tb_FontSize / 4.0) / myFonts.Tb_FontSize * myFonts.CoeffFont_Widh)
       
                // ...Start position refresh each movment till press SHIFT
                do this.Dispatcher.Invoke(new Action ( fun () -> do canvasSelecting.Children.Clear()))
                if Keyboard.Modifiers <> ModifierKeys.Shift && blnMouseLeftPressed  then                   
                    
                    do intAbsolutSelectVertStart  <- crt.AbsoluteNumLineCurrent   // Save/Select Start Position for Selection
                    do intAbsolutSelectHorizStart <- crt.AbsoluteNumCharCurrent
                    do intAbsolutSelectVertCurrent <- intAbsolutSelectVertStart
                    do intAbsolutSelectHorizCurrent <- intAbsolutSelectHorizStart
                else
                    do intAbsolutSelectVertCurrent  <- crt.AbsoluteNumLineCurrent    // Save/Select Current Position for Selection
                    do intAbsolutSelectHorizCurrent <- crt.AbsoluteNumCharCurrent    
        
                do this.Dispatcher.Invoke(new Action ( fun () -> do set_Caret()))
       

                if Keyboard.Modifiers = ModifierKeys.Shift || Mouse.LeftButton = MouseButtonState.Pressed && blnUpdateReady  then  //Mouse.LeftButton = MouseButtonState.Pressed 
                    if intAbsolutSelectVertStart <> intAbsolutSelectVertCurrent ||
                       intAbsolutSelectHorizStart <> intAbsolutSelectHorizCurrent
                    then do currentcurrentCanvasSelecting() 
  
            else ignore()
  
        


    let mouseLeftDown(e: MouseButtonEventArgs) =
        //do intAbsolutSelectVertStart  <- crt.AbsoluteNumLineCurrent   // Save/Select Start Position for Selection
        //do intAbsolutSelectHorizStart <- crt.AbsoluteNumCharCurrent

        //do intAbsolutSelectVertCurrent <- intAbsolutSelectVertStart
        //do intAbsolutSelectHorizCurrent <- intAbsolutSelectHorizStart

        do this.Dispatcher.Invoke(new Action ( fun () -> do canvasSelecting.Children.Clear()))
        do blnMouseLeftPressed <- true
        do setMousePositionForMoving()      

    let mouseLeftUp(e: MouseButtonEventArgs) = 
        do blnMouseLeftPressed <- false
        do intAbsolutSelectVertCurrent <- crt.AbsoluteNumLineCurrent   // Save/Select Start Position for Selection
        do intAbsolutSelectHorizCurrent <- crt.AbsoluteNumCharCurrent // Save/Select Start Position for Selection 


    let mouseRightDown(e: MouseButtonEventArgs) = ()
    let mouseRightUp(e: MouseButtonEventArgs) =   ()


   
   
   
   
    //let mouseEnter(e:MouseEventArgs) = if e.MouseDevice.LeftButton.ToString() = "Pressed" then flagLeftDown <- true
    //                                                                                      else flagLeftDown <- false
    //                                   if e.MouseDevice.RightButton.ToString() = "Pressed" then flagRightDown <- true
    //                                                                                       else flagRightDown <- false
    //                                   match Keyboard.Modifiers with
    //                                   | ModifierKeys.Shift   -> flagShift <- true
    //                                                             flagAlt <- false
    //                                                             flagControl <- false 

    //                                   | ModifierKeys.Alt -> flagShift <- false
    //                                                         flagAlt <- true
    //                                                         flagControl <- false 

    //                                   | ModifierKeys.Control -> flagShift <- false
    //                                                             flagAlt <- false
    //                                                             flagControl <- true 

    //                                   | _ -> flagShift <- false
    //                                          flagAlt <- false
    //                                          flagControl <- false 
                                     

  
  
    let mouseMove(e:MouseEventArgs) =           

            if Keyboard.Modifiers = ModifierKeys.Shift || Mouse.LeftButton = MouseButtonState.Pressed  then           //&& Keyboard.Modifiers = ModifierKeys.Shift
                 
                if crt.AbsoluteNumLineCurrent >= openUpdateMMF.IntLastLineOnPage - 1  then 
                                                        do scrollY.Value <- scrollY.Value + 1.0 //3.0
                                                        do crt.AbsoluteNumLineCurrent <- (int)scrollY.Value + openUpdateMMF.IntVertCountLinesOnPage
                                                        do blnUpdateReady <- false
                if crt.AbsoluteNumLineCurrent <= openUpdateMMF.IntFirstLineOnPage then 
                                                        do scrollY.Value <- scrollY.Value - 1.0 // 3.0
                                                        do crt.AbsoluteNumLineCurrent <- (int)scrollY.Value
                                                        do blnUpdateReady <- false
                if crt.AbsoluteNumCharCurrent >= openUpdateMMF.IntLastCharOnPage  then 
                                                        do scrollX.Value <- scrollX.Value + 1.0 // 3.0
                                                        do crt.AbsoluteNumCharCurrent <- (int)scrollX.Value + openUpdateMMF.IntHorizCountCharsOnPage
                                                        do blnUpdateReady <- false
                if crt.AbsoluteNumCharCurrent <= openUpdateMMF.IntFirstCharOnPage then 
                                                        do scrollX.Value <- scrollX.Value - 1.0 // 3.0
                                                        do crt.AbsoluteNumCharCurrent <- (int)scrollX.Value
                                                        do blnUpdateReady <- false
                do Thread.Sleep(10)               
                do setMousePositionForMoving()  

            // Inform Section - Status

            let intRelativeX = (int)((Mouse.GetPosition(canvasSelected).X + myFonts.Tb_FontSize / 4.0) * myFonts.CoeffFont_Widh / myFonts.Tb_FontSize)
            let intRelativeY = (int)(Mouse.GetPosition(canvasSelected).Y  * myFonts.CoeffFont_High / myFonts.Tb_FontSize)
            
            do this.Dispatcher.Invoke(new Action ( fun () -> tbXY.Text <- "X:" + (intRelativeX + openUpdateMMF.IntFirstCharOnPage).ToString("0,0") 
                                                                               + "   Y:" + (intRelativeY + openUpdateMMF.IntFirstLineOnPage).ToString("0,0")))
            
            
            //do Keyboard.Focus(crt) |> ignore       // focus caretCanvas (textbox) itself
            //do crt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down)) |> ignore    // move focus down to caret inside caretCanvas
            //do Thread.Sleep(0) 




    let initXScroll() = 
            do openUpdateMMF.IntFirstCharOnPage <- (int)scrollX.Value
            do openUpdateMMF.IntHorizCountCharsOnPage <- (int)(canvasSelected.ActualWidth * myFonts.CoeffFont_Widh / myFonts.Tb_FontSize )
            do openUpdateMMF.IntHorizCountCharsOnPage <- match openUpdateMMF.IntHorizCountCharsOnPage < 0 with | true -> 0 | false -> openUpdateMMF.IntHorizCountCharsOnPage
            do scrollX.Minimum <- 0.0
            do scrollX.Maximum <- match openUpdateMMF.IntMaxCharsInLine <= 0 with | true ->  0.0 | false -> float (openUpdateMMF.IntMaxCharsInLine ) // size of longest line
            do scrollX.SmallChange <- 1.0   
            do scrollX.LargeChange <- float (openUpdateMMF.IntHorizCountCharsOnPage / 2);
            do tbX.Text <- "X: " + openUpdateMMF.IntFirstCharOnPage.ToString("0,0") + " of " + ((int)scrollX.Maximum).ToString("0,0") 
            do Thread.Sleep(0)
            


    let initYScroll() = 
            do openUpdateMMF.IntFirstLineOnPage <- (int)scrollY.Value
            do openUpdateMMF.IntVertCountLinesOnPage <- (int)(canvasSelected.ActualHeight * myFonts.CoeffFont_High / myFonts.Tb_FontSize )

            do scrollY.Maximum <- float openUpdateMMF.IntNumberOfTotalLinesEstimation
            do scrollY.SmallChange <- 1.0   // one line
            do scrollY.LargeChange <- float (openUpdateMMF.IntVertCountLinesOnPage / 2)   // number lines per page/2
            do tbY.Text <- "Y: " + openUpdateMMF.IntFirstLineOnPage.ToString("0,0") + " of " + ((int)scrollY.Maximum).ToString("0,0") + "   (" + openUpdateMMF.IntVertCountLinesOnPage.ToString() + ")";     
            do Thread.Sleep(0)
            


    let updateUserClock (blnVisible : bool) =

            this.Dispatcher.InvokeAsync( fun _ ->  
                 match blnVisible with
                 | true ->  userClock.Visibility <- Visibility.Visible 
                            canvasMain.AllowDrop <- false         
                 | false -> userClock.Visibility <- Visibility.Collapsed
                            canvasMain.AllowDrop <- true ) |> ignore
            do Thread.Sleep(0)
    

    let update(blnChange : bool) =  
        [ async {
            this.Dispatcher.InvokeAsync(new Action ( fun _ -> 
                 do initXScroll()
                 if blnChange then do initYScroll() 
                 set_Caret())) |> ignore

            this.Dispatcher.InvokeAsync(new Action ( fun _ ->                                                    
                 do openUpdateMMF.UpdateCurrentWindow(ref txtBlock  , ref lenghtArr, blnChange, myMenu.TxtFind) |> ignore
                 //if myMenu.TxtFind.Trim() = "" then do openUpdateMMF.BlnStopSearch <- true 
                 set_Caret()
                 do blnUpdateReady <- true)) |> ignore                                            

                  } ] |> Async.Parallel |> Async.Ignore |> Async.Start                                  

                                                                          
 
    let updateDoubleY() = 
            this.Dispatcher.InvokeAsync(new Action ( fun _ ->
                 do openUpdateMMF.BlnStopSearch <- true
                 do updateUserClock(false) 
                 Thread.Sleep(0)
                 let deltaY = scrollX.ActualHeight
                 let curr = Mouse.GetPosition(scrollY)
                 let iy = int (scrollY.Maximum *  (curr.Y - deltaY) / (scrollY.ActualHeight - 2.0 * deltaY) )
                 Thread.Sleep(0)
                 scrollY.Value <- float iy
                 
                 if (int)scrollY.Value >= openUpdateMMF.IntNumberOfTotalLinesEstimation then
                     do scrollY.Value <- float openUpdateMMF.IntNumberOfTotalLinesEstimation - 1.0

                 )) |> ignore 
                                                            

    let updateDoubleX() =
            this.Dispatcher.InvokeAsync(new Action ( fun _ ->
                 do openUpdateMMF.BlnStopSearch <- true
                 do updateUserClock(false) 
                 Thread.Sleep(0)

                 let deltaX = scrollY.ActualWidth
                 let curr = Mouse.GetPosition(scrollX)
                 let ix = int (scrollX.Maximum *  (curr.X - deltaX) / (scrollX.ActualWidth - 2.0 * deltaX))
                 Thread.Sleep(0)
                 scrollX.Value <- float ix 
                 )) |> ignore

 

    let eventSysInfoStart(arg) = 
            this.Dispatcher.Invoke (DispatcherPriority.ContextIdle, new Action(fun _ ->
                 userClock.Visibility <- Visibility.Collapsed 
                 (!statusBar).PrgStatusValue <- arg 
                 (!statusBar).NumberTotalLines <- openUpdateMMF.IntNumberOfTotalLinesEstimation
                 
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
            do Thread.Sleep(0)  

    let scrolXWheel (e : MouseWheelEventArgs) =
            if e.Delta > 0 then scrollX.Value <- float(openUpdateMMF.IntFirstCharOnPage - 10)
                           else scrollX.Value <- float(openUpdateMMF.IntFirstCharOnPage + 10)
            


    let scrolYWheel (e : MouseWheelEventArgs) =
            
            if e.Delta > 0 then scrollY.Value <- float(openUpdateMMF.IntFirstLineOnPage - 5)
                           else scrollY.Value <- float(openUpdateMMF.IntFirstLineOnPage + 5)
 


    let canvasWheel (e : MouseWheelEventArgs) =
            match Keyboard.Modifiers with
            | ModifierKeys.Shift -> scrolXWheel(e)
            | ModifierKeys.Control -> ()
            | _ -> scrolYWheel(e)
            do Thread.Sleep(0)



    let isCrtInsideWindow() = 

           let posX = match crt.AbsoluteNumCharCurrent with
                      | x when x < (openUpdateMMF.IntFirstCharOnPage ) -> -1
                      | x when x > (openUpdateMMF.IntLastCharOnPage  ) -> +1
                      | _ -> 0

           let posY = match crt.AbsoluteNumLineCurrent with
                      | y when y < (openUpdateMMF.IntFirstLineOnPage ) -> -1
                      | y when y > (openUpdateMMF.IntLastLineOnPage ) -> +1
                      | _ -> 0 

           match posX, posY with   
           // 0,  0   is default behavior.

           | -1,  0 -> scrollX.Value <- (float)crt.AbsoluteNumCharCurrent                                        
           
           |  0, -1 -> scrollY.Value <- (float)crt.AbsoluteNumLineCurrent
           
           | -1, -1 -> scrollX.Value <- (float)crt.AbsoluteNumCharCurrent
                       scrollY.Value <- (float)crt.AbsoluteNumLineCurrent
           
           | +1,  0 -> scrollX.Value <- (float)(crt.AbsoluteNumCharCurrent - openUpdateMMF.IntHorizCountCharsOnPage)                       
           
           |  0, +1 -> scrollY.Value <- (float)(crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage + 1) 
           
           | +1, +1 -> scrollX.Value <- (float)(crt.AbsoluteNumCharCurrent - openUpdateMMF.IntHorizCountCharsOnPage)
                       scrollY.Value <- (float)(crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage + 1)
           
           | +1, -1 -> scrollX.Value <- (float)(crt.AbsoluteNumCharCurrent - openUpdateMMF.IntHorizCountCharsOnPage)
                       scrollY.Value <- (float)crt.AbsoluteNumLineCurrent

           | -1, +1 -> scrollX.Value <- (float)crt.AbsoluteNumCharCurrent
                       scrollY.Value <- (float)(crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage + 1)
           | _ ,  _ -> ignore()

           (posX, posY)




    let canvasKeyDown (e : KeyEventArgs ) =
 
            match Keyboard.Modifiers with
            | ModifierKeys.Shift   -> match e.Key with
                                      | Key.Tab -> scrollX.Value <-  scrollX.Value - 4.0       

                                      | Key.PageUp -> scrollY.Value <- scrollY.Value - float openUpdateMMF.IntVertCountLinesOnPage * 10.0        // 100*Up

                                      | Key.PageDown -> scrollY.Value <- scrollY.Value + float openUpdateMMF.IntVertCountLinesOnPage * 10.0      // 100*Down

                                    //  | Key.Home -> scrollX.Value <- 0.0
                                    
                                      | _ -> ignore()     // Left one Page

            
            | ModifierKeys.Alt     -> match e.Key with   // System Only
                                      | _ -> ignore()  

            | ModifierKeys.Control -> match e.Key with
                                      | Key.Home -> crt.AbsoluteNumLineCurrent <- 0
                                                    crt.AbsoluteNumCharCurrent <- 0
                                                    do isCrtInsideWindow() |> ignore

                                      | Key.End  -> crt.AbsoluteNumLineCurrent <- openUpdateMMF.IntNumberOfTotalLinesEstimation                                                 
                                                    do isCrtInsideWindow() |> ignore

                                      | Key.PageUp   -> crt.AbsoluteNumLineCurrent <- crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage * 10 
                                                        if crt.AbsoluteNumLineCurrent < 0 then crt.AbsoluteNumLineCurrent <- 0
                                                        do isCrtInsideWindow() |> ignore

                                      | Key.PageDown -> crt.AbsoluteNumLineCurrent <- crt.AbsoluteNumLineCurrent + openUpdateMMF.IntVertCountLinesOnPage * 10 
                                                        if crt.AbsoluteNumLineCurrent > openUpdateMMF.IntNumberOfTotalLinesEstimation then crt.AbsoluteNumLineCurrent <- openUpdateMMF.IntNumberOfTotalLinesEstimation
                                                        do isCrtInsideWindow() |> ignore

                                      | Key.O        -> myMenu.Visibility <- Visibility.Visible
                                      | Key.F        -> openFind.Trigger()
                                      | _ -> ignore()


            | _ ->                    match e.Key with
                                      | Key.Escape -> openUpdateMMF.BlnStopSearch <- true
                                      | Key.PageUp -> match isCrtInsideWindow() with
                                                        | 0 , 0 -> if crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage >=0
                                                                      then do crt.AbsoluteNumLineCurrent  <- crt.AbsoluteNumLineCurrent - openUpdateMMF.IntVertCountLinesOnPage
                                                                           do scrollY.Value <- scrollY.Value - float openUpdateMMF.IntVertCountLinesOnPage
                                                        | _ , _ -> ignore()
                                      | Key.PageDown -> match isCrtInsideWindow() with
                                                        | 0 , 0 -> do crt.AbsoluteNumLineCurrent  <- crt.AbsoluteNumLineCurrent + openUpdateMMF.IntVertCountLinesOnPage
                                                                   scrollY.Value <- scrollY.Value + float openUpdateMMF.IntVertCountLinesOnPage
                                                        | _ , _ -> ignore()
                                                              // Down
                                      | Key.Home ->  match isCrtInsideWindow() with
                                                     | 0 , 0 -> do crt.AbsoluteNumCharCurrent  <- 0
                                                                do scrollX.Value <- 0.0
                                                     | _ , _ -> ignore()
                                                    
                                      | Key.End ->   match isCrtInsideWindow() with
                                                     | 0 , 0 -> let indexArr = crt.AbsoluteNumLineCurrent - openUpdateMMF.IntFirstLineOnPage  
                                                                crt.AbsoluteNumCharCurrent <- lenghtArr.[indexArr] 
                                                                scrollX.Value <- (float) (crt.AbsoluteNumCharCurrent - openUpdateMMF.IntHorizCountCharsOnPage)
                                                     | _ , _ -> ignore()

                                                                      
                                      | Key.Left ->  if crt.AbsoluteNumCharCurrent > 0 then 
                                                        do crt.AbsoluteNumCharCurrent <- crt.AbsoluteNumCharCurrent - 1                                     
                                                     do isCrtInsideWindow() |> ignore

                                      | Key.Right -> do crt.AbsoluteNumCharCurrent <- crt.AbsoluteNumCharCurrent + 1 
                                                     do isCrtInsideWindow() |> ignore
                                                     
                                      | Key.Up ->    if crt.AbsoluteNumLineCurrent > 0 then 
                                                        do crt.AbsoluteNumLineCurrent <- crt.AbsoluteNumLineCurrent - 1  
                                                     do isCrtInsideWindow() |> ignore   
                                                     
                                      | Key.Down ->  do crt.AbsoluteNumLineCurrent <- crt.AbsoluteNumLineCurrent + 1 
                                                     do isCrtInsideWindow() |> ignore

                                      | Key.Tab ->   crt.AbsoluteNumCharCurrent <- crt.AbsoluteNumCharCurrent + 4 
                                                     do isCrtInsideWindow() |> ignore

                                      | Key.Insert -> blnInsert <- not blnInsert
                                                      do isCrtInsideWindow() |> ignore
                                      | _ when ((int)e.Key >= 18) ->  do isCrtInsideWindow() |> ignore

                                      | _ -> ignore()

            do this.Dispatcher.Invoke(new Action ( fun () -> do set_Caret()))
            do Thread.Sleep(0)


    let canvasKeyUp (e : KeyEventArgs ) =                 
        do Thread.Sleep(0)


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
                                                                                          //do eventSysInfoUpdate.Trigger(scale)
                                                                                          ))

                              | x ->     do this.Dispatcher.Invoke(new Action ( fun () -> do lblDropHere.Visibility <- Visibility.Visible
                                                                                          do lblDropHere.Content <- x ))
                    true
            
            // Final Thread
            let finalThreadDoWOrk(x : bool) = 
                                    do this.Dispatcher.Invoke(new Action ( fun () -> 

                                                do updateUserClock (false)
                                                do update(true)
                                                Thread.Sleep(0) |> ignore))
            
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
                                                                                           Thread.Sleep(0)     
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
 
 
 
 

    let crtEventTextInput(e : TextCompositionEventArgs) = 
        
        let chr = e.Text
        
        ignore()   // crt.LastChar
  
  
  
    let mutable blnCrtFocus = false 
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
 
            do canvasMain.SizeChanged.Add(fun e -> update(true)) 
            do canvasMain.Drop.Add(fun e -> openFileDrag(e))
            do canvasMain.Unloaded.Add(fun e -> unLoaded(e))
           
            do canvasMain.MouseMove.Add(fun e -> mouseMove(e)) 
            
            //do canvasMain.MouseEnter.Add(fun e -> mouseEnter(e))
            
            do canvasMain.MouseWheel.Add(fun e -> canvasWheel(e)) 

            do canvasMain.MouseLeftButtonDown.Add(fun e -> mouseLeftDown(e) ) //setMousePositionForMoving()
            do canvasMain.MouseLeftButtonDown.Add(fun e -> mouseLeftUp(e) )
            do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightDown(e) ) //setMousePositionForMoving()
            do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightUp(e) )            

            //do canvasMain.KeyUp.Add(fun e ->   canvasKeyUp (e))   // Drop all flags “shift”, “alt”, “controls” only                                                 
            //do crt.EventTxtKeyUp.Add(fun e ->  do canvasKeyUp (e))  


            //do canvasMain.MouseRightButtonDown.Add(fun e -> mouseRightDown(e))

            // DO NOT USE TWO BELOW EVENTS FOR CATCH CHAR/INPUT
            do canvasMain.KeyDown.Add(fun e ->  if blnCrtFocus  // Prevent of duplicate action // If cursor, not visible will be work this code...
                                                   then blnCrtFocus <-false
                                                   else do canvasKeyDown (e)) 
            
            do crt.EventTxtKeyDown.Add(fun e -> do blnCrtFocus <- true   // Prevent of duplicate action 
                                                do canvasKeyDown (e))    // it works if cursor is visible.

            // CATCH INPUT CHAR
            do crt.EventTextInput.Add(fun e -> crtEventTextInput (e))    // Use this event for catch new char with right encoding.

            do this.Dispatcher.Invoke(new Action ( fun () -> do set_Caret()))
            do Thread.Sleep(0)
       
            //do crt.Focusable <- true
            //do Keyboard.Focus(crt) |> ignore       // focus caretCanvas (textbox) itself
            //do crt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down)) |> ignore    // move focus down to caret inside caretCanvas


            //do Touch.FrameReported.AddHandler( fun sender e -> myTouch.TouchFrame(sender, e))
            //do myTouch.MyCanvas <- ref canvasMain
            //do myTouch.EventScale.Add(fun e -> scaleCurrnet.Trigger(e)) 




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
    member x.ScaleCurrnet =   scaleCurrnet.Publish
    [<CLIEvent>]
    member x.OpenFind = openFind.Publish

    member x.StatusBar with get() = statusBar and set(v)= statusBar <- v 

    member x.IntFirstCharOnPage with get() = (int)scrollX.Value and set(v) = scrollX.Value <- (double)v
    member x.IntFirstLineOnPage with get() = (int)scrollY.Value and set(v) = scrollY.Value <- (double)v

    member x.OpenUpdateMMF with get() = openUpdateMMF   

    member x.BlnPlaceHolder with get() = blnPlaceHolder and set(v) = blnPlaceHolder <-v
    member x.BlnPenDeSelect with get() = blnPenDeSelect and set(v) = blnPenDeSelect <-v
    member x.BlnRecSelect with get() = blnRecSelect and set(v) = blnRecSelect <-v
    member x.BlnRecDeSelect with get() = blnRecDeSelect and set(v) = blnRecDeSelect <-v











 
                                





