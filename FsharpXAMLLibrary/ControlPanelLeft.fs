namespace FsharpXAMLLibrary

open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
    
open  System.Windows
open  System.Windows.Shapes
open  System.Windows.Controls
open  System.Windows.Controls.Primitives
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


type ControlPanelLeft()  as this = 
    inherit  UserControl()

    do this.Content <- contentAsXamlObjectFromAssembly("FsharpXAMLLibrary","ControlPanelLeft") // Load XAML

    let mutable btnAppearance : Button =  this.Content?btnAppearance 
    let mutable btnGroupOperation : Button =  this.Content?btnGroupOperation 
    let mutable btnHandArrow : Button =  this.Content?btnHandArrow 
    
    //Group SELECT
    let mutable grpSelect : GroupBox =  this.Content?grpSelect 
    let mutable btnSelectAll : Button =  this.Content?btnSelectAll 
    let mutable btnDeSelectAll : Button =  this.Content?btnDeSelectAll 
    let mutable btnLeftUp : Button =  this.Content?btnLeftUp 
    let mutable btnRightDown : Button =  this.Content?btnRightDown 
    let mutable btnSelectLine : Button =  this.Content?btnSelectLine 
    let mutable btnLineLeft : Button =  this.Content?btnLineLeft 
    let mutable btnLineRight : Button =  this.Content?btnLineRight 

    let mutable btnFindReplace : Button =  this.Content?btnFindReplace 

    //Group DESELECT
    let mutable grpDeSelect : GroupBox =  this.Content?grpDeSelect 
    let mutable btnDeLeftUp : Button =  this.Content?btnDeLeftUp 
    let mutable btnDeRightDown : Button =  this.Content?btnDeRightDown 
    let mutable btnDeSelectLine : Button =  this.Content?btnDeSelectLine 
    let mutable btnDeLineLeft : Button =  this.Content?btnDeLineLeft 
    let mutable btnDeLineRight : Button =  this.Content?btnDeLineRight 

    //Group Special
    let mutable grpSpecial : GroupBox =  this.Content?grpSpecial 
    let mutable btnPenDeSelect : Button =  this.Content?btnPenDeSelect
    let mutable btnRecDeSelect : Button =  this.Content?btnRecDeSelect 
    let mutable btnRecSelect : Button =  this.Content?btnRecSelect 
    let mutable btnPlaceHolder : Button =  this.Content?btnPlaceHolder     
   
   //Group FORMAT
    let mutable grpFormat : GroupBox =  this.Content?grpFormat 
    let mutable btnFormat : Button =  this.Content?btnFormat 
    let mutable btnDeFormat : Button =  this.Content?btnDeFormat 
    let mutable btnFormatByPunctuationChar : Button =  this.Content?btnFormatByPunctuationChar 
    let mutable btnRemoveContinuouslySpace : Button =  this.Content?btnRemoveContinuouslySpace



//    let systemInfo() =  let myComputer =  new Microsoft.VisualBasic.Devices.ComputerInfo() 
//                        let totalAvailable  = myComputer.AvailablePhysicalMemory 
//                        let mutable intOS = 32
//                        if Environment.Is64BitOperatingSystem then intOS <- 64
//                        let intTotalMemory = System.GC.GetTotalMemory(false)
//                        do statusSystemBar.Content <- "OS:X" + intOS.ToString() + " Used Memory - " + intTotalMemory.ToString("0,0") + " ( Available " + totalAvailable.ToString("0,0") + " )";
//                        do statusDocSizeBar.Content <- " ( " + longTotalDocSize.ToString() + " bytes) ";
//  
//    do systemInfo()
//
//    // Add Event - Selection Changed
//    let eventStatusSystem = new Event<MouseEventArgs>() 
//    do statusSystem.MouseLeftButtonDown.Add(fun e ->
//                                                 do systemInfo() 
//                                                 eventStatusSystem.Trigger(e)
//                                           )
//
//    member x.EventStatusSystem  =  eventStatusSystem.Publish 
//    member x.LongTotalDocSize  with set(v) = longTotalDocSize <- v
//                                             do systemInfo()


