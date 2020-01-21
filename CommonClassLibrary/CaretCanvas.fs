namespace CommonClassLibrary

open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
    
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

open Utilities

open Microsoft.FSharp.Control

type  CaretCanvas()  as this = 
    inherit  UserControl()
    // Load CaretTxt.xaml

    //do this.Content <- contentAsXamlObject("CaretCanvas") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("CommonClassLibrary","CaretCanvas") // Load XAML


    let mutable myCaret : TextBox = this.Content?myCaret 
    let mutable tr : TranslateTransform = this.Content?tr
    let mutable lastChar : String = "" 

    do myCaret.TextWrapping <- TextWrapping.NoWrap
    do myCaret.UndoLimit <- 0
    do myCaret.IsUndoEnabled <-false
    do myCaret.ContextMenu <- new ContextMenu()
    do myCaret.Foreground <- myCaret.Background
    do myCaret.CaretBrush <- myCaret.Background 
    
  
  // Application code handle it ...
    do DataObject.AddPastingHandler (myCaret, new DataObjectPastingEventHandler(fun o e -> e.CancelCommand()))
    do DataObject.AddCopyingHandler (myCaret, new DataObjectCopyingEventHandler(fun o e -> e.CancelCommand()))

  //http://www.codeproject.com/Questions/88171/Disallowing-paste-Ctrl-V-in-a-WPF-TextBox
  //http://weblogs.asp.net/marianor/how-to-disable-copy-and-paste-for-a-control-in-wpf
     
    let mutable intAbsoluteNumLineCurrent = 0  // absolute Line
    let mutable intAbsoluteNumCharCurrent = 0  // absolute Char

    // Creates event / Invokes event Preview Key Down
    let keyDown = new Event<KeyEventArgs>()
    do myCaret.PreviewKeyDown.Add(fun e -> keyDown.Trigger(e))

    // Creates event / Invokes event Preview Key Up
    let keyUp = new Event<KeyEventArgs>()
    do myCaret.PreviewKeyUp.Add(fun e -> keyUp.Trigger(e))
    
    let textInput = new Event<TextCompositionEventArgs>()
    do myCaret.PreviewTextInput.Add(fun e -> 
                 do lastChar <- e.Text // One char or maybe String if paste
                 do myCaret.Clear()
                 do myCaret.CaretIndex <- 0 // After each INPUT clear textBox (it only using for translate inputs to CHARS)
                 textInput.Trigger(e)
                                   ) 

    // Exposed event handler
    member x.EventTxtKeyDown =  keyDown.Publish
    member x.EventTxtKeyUp   =  keyUp.Publish
    member x.EventTextInput  =  textInput.Publish

    member x.LastChar with get() = lastChar
   
    member x.AbsoluteNumLineCurrent  with get() = intAbsoluteNumLineCurrent    and set(v) = intAbsoluteNumLineCurrent <- v
    member x.AbsoluteNumCharCurrent  with get() = intAbsoluteNumCharCurrent    and set(v) = intAbsoluteNumCharCurrent  <- v

    member x.FloatCareteW  with get() = myCaret.Width  and set(v) = myCaret.Width   <- v
    member x.FloatCareteH  with get() = myCaret.Height  and set(v) = myCaret.Height   <- v

    member x.BackGroundColorCarete with get() = myCaret.Background and set(v) = myCaret.Background <- v; 
                                                                                myCaret.Foreground <- v; 
                                                                                myCaret.CaretBrush <- v;

    member x.TranslateTransform with get() = tr and set(v) = tr <- v 


    

    
 

