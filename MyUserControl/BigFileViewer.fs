namespace MyUserControl

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
open System.Collections.Generic
open System.Windows.Media
open System.Windows.Media.Imaging
open System.Windows.Input
open CommonClassLibrary

open FsharpXAMLLibrary
open FsharpXAMLLibrarySupport


open System.Windows.Threading
open Utilities

type BigFileViewer() as this  = 
    inherit UserControl() 

    //do this.Content <- contentAsXamlObject("BigFileViewer") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("MyUserControl","BigFileViewer") // Load XAML

    
    // FIND ALL OBJECTS IN THIS.CONTENT  :StatusBarSystem x:Name="statusBar"
    let mutable myControlPanelLeft : ControlPanelLeft = (this.Content)?controlPanelLeft
    let mutable myTextBox : MyTextBox = (this.Content)?myTextBox 
    let mutable statusBar : StatusBarSystem = (this.Content)?statusBar 

    let mutable allScale : ScaleTransform = (this.Content)?allScale
    let mutable bigGrid : Grid  = (this.Content)?bigGrid
    let mutable winHolder : Window = new Window()

    let mutable listOfWindows = new List<Window>()

    let minHeight = 770.0
    let minWidth = 512.0

    let mutable scale = 1.0
    let scalePlus() =  if scale < 1.5 then scale <- scale + 0.05
                       statusBar.ScaleNewValue <- scale
                       allScale.ScaleX <- scale
                       allScale.ScaleY <- scale


    let scaleMinus() = if scale > 0.70 then scale <- scale - 0.05
                       statusBar.ScaleNewValue <- scale
                       allScale.ScaleX <- scale
                       allScale.ScaleY <- scale
                                                 

    let canvasWheel (e : MouseWheelEventArgs) =
            match Keyboard.Modifiers with
            | ModifierKeys.Control -> if e.Delta > 0 then scalePlus() 
                                                     else scaleMinus();
            | _ -> ()
            winHolder.MinHeight <- minHeight * scale 
            winHolder.MinWidth <- minWidth * scale 

    let scaleCurrnet(e : float) = 
            match e with
            | _ when e > 0.0 -> allScale.ScaleX <- e
                                allScale.ScaleY <- e
            | _ -> ignore ()
    
    do myTextBox.StatusBar <- ref statusBar
   
    do myTextBox.ScaleCurrnet.Add(fun e ->  scaleCurrnet(e))

    do this.MouseWheel.Add(fun e -> canvasWheel(e))
    
 
    do myControlPanelLeft.MyTextBox <- ref myTextBox


    let deltaAdjVert =  4.0 * (float)System.Windows.SystemParameters.Border
                      + 4.0 * (float)System.Windows.SystemParameters.FixedFrameHorizontalBorderHeight 
                      + System.Windows.SystemParameters.WindowCaptionHeight + 2.0


    let deltaAdjHoriz = 4.0 * (float)System.Windows.SystemParameters.Border
                      + 4.0 * (float)System.Windows.SystemParameters.FixedFrameVerticalBorderWidth + 2.0




    let createNewFind() =
        let mutable quickFind = new QuickFind()
        do quickFind.InitMyTextBox(ref myTextBox) 

 //http://reedcopsey.com/2011/11/28/launching-a-wpf-window-in-a-separate-thread-part-1/
 //https://stackoverflow.com/questions/33379559/f-sta-thread-async

  // let cancellationSource = new CancellationTokenSource()
    
    let openFindDialog() = 
           let newWindowThread = async {
            
                let win = new Window()                
        
                let uc = new QuickFind()
                do uc.InitMyTextBox(ref myTextBox)
        
                do win.Content <- uc
                do win.Height <- 200.0
                do win.Width <- 500.0
                do win.Name <- "QuickFind"
                do win.Title <- "Quick Find"                                                    
                do win.Height <- 65.0
                
                do win.Closing.Add(fun _ -> uc.BlnStopSearch <- true
                                            myTextBox.BtnCommand("StopAll"))
                //do win.Closed.Add(fun _ -> Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.ApplicationIdle ))

                do win.Show()    
           }  
           Async.StartImmediate(newWindowThread)   //, cancellationSource.Token)
          

    do myControlPanelLeft.FindReplace.Add(fun _ -> openFindDialog()) 
    do myTextBox.OpenFind.Add(fun _ -> openFindDialog())

    // Synchronized UserControl size with Window
    member x.WinHolder  with set(v) = ( do winHolder <- v 
                                        do this.Width <- v.ActualWidth -   deltaAdjHoriz 
                                        do this.Height <- v.ActualHeight - deltaAdjVert 
                                     )





 
                                





