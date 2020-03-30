namespace FsharpXAMLLibrarySupport

open System
open System.Collections.ObjectModel

open System.Windows
open System.Windows.Controls
    
open  System.Windows
open  System.Windows.Shapes

open  System.Windows.Controls.Primitives
open  System.Windows.Input
//open  System.Windows.Media
//open  System.Windows.Media.Imaging
open  System.Windows.Threading

open System.IO
open System.Windows.Markup
open System.Xaml
open System.Reflection

open Utilities
open Microsoft.FSharp.Control
open System.Windows
open FsharpXAMLLibrary


type ControlPanelLeft()  as this = 
    inherit  UserControl()

    do this.Content <- contentAsXamlObjectFromAssembly("FsharpXAMLLibrarySupport","ControlPanelLeft") // Load XAML

    //let mutable btnAppearance : Button =  this.Content?btnAppearance 
    let mutable btnGroupOperation : Button =  this.Content?btnGroupOperation 
    //let mutable btnHandArrow : Button =  this.Content?btnHandArrow 
    
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
    //let mutable grpSpecial : GroupBox =  this.Content?grpSpecial 
    let mutable btnPenDeSelect : Button =  this.Content?btnPenDeSelect
    let mutable btnRecDeSelect : Button =  this.Content?btnRecDeSelect 
    let mutable btnRecSelect : Button =  this.Content?btnRecSelect 
    let mutable btnPlaceHolder : Button =  this.Content?btnPlaceHolder     
    let mutable btnCopyGroup : Button =  this.Content?btnCopyGroup 

    let mutable myTextBox  = ref (new MyTextBox())

   //Group FORMAT
    //let mutable grpFormat : GroupBox =  this.Content?grpFormat 
    //let mutable btnFormat : Button =  this.Content?btnFormat 
    //let mutable btnDeFormat : Button =  this.Content?btnDeFormat 
    //let mutable btnFormatByPunctuationChar : Button =  this.Content?btnFormatByPunctuationChar 
    //let mutable btnRemoveContinuouslySpace : Button =  this.Content?btnRemoveContinuouslySpace

    // It is for FREE version
    do btnGroupOperation.IsEnabled <- false
    do btnPlaceHolder.IsEnabled <- false
    

    let colorChange(btn : Button ref, brDef : Media.SolidColorBrush   ) = 
            if btn.Value.Foreground = (brDef :> Media.Brush)
            then btn.Value.Foreground <- System.Windows.Media.Brushes.Red
            else btn.Value.Foreground <- brDef
    
    let blnColorChange(btn : Button ref) = 
            if btn.Value.Foreground = (Media.Brushes.DarkBlue :> Media.Brush) ||  btn.Value.Foreground = (Media.Brushes.White :> Media.Brush)
            then false
            else true

    let updateBooleanMyTextBox() =
            do myTextBox.Value.BlnGroupOperation <- blnColorChange(ref btnGroupOperation)
            do myTextBox.Value.BlnPlaceHolder <- blnColorChange(ref btnPlaceHolder)
            do myTextBox.Value.BlnRecSelect <- blnColorChange(ref btnRecSelect)
            do myTextBox.Value.BlnPenDeSelect <- blnColorChange(ref btnPenDeSelect)
            do myTextBox.Value.BlnRecDeSelect <- blnColorChange(ref btnRecDeSelect)

    do btnGroupOperation.Click.Add(fun _ -> colorChange(ref btnGroupOperation, Media.Brushes.DarkBlue)  |> ignore
                                            do grpDeSelect.IsEnabled <- blnColorChange (ref btnGroupOperation)
                                            do btnRecSelect.Foreground <- Media.Brushes.DarkBlue
                                            do btnPenDeSelect.Foreground <- Media.Brushes.White
                                            do btnRecDeSelect.Foreground <- Media.Brushes.White
                                            do btnPlaceHolder.Foreground <- Media.Brushes.DarkBlue
                                            do updateBooleanMyTextBox())

    do btnDeLeftUp.Click.Add(fun _    -> myTextBox.Value.BtnCommand("DeLeftUp")) 
    do btnDeLineLeft.Click.Add(fun _  -> myTextBox.Value.BtnCommand("DeLineLeft")) 
    do btnDeLineRight.Click.Add(fun _ -> myTextBox.Value.BtnCommand("DeLineRight")) 
    do btnDeRightDown.Click.Add(fun _ -> myTextBox.Value.BtnCommand("DeRightDown"))
    do btnDeSelectAll.Click.Add(fun _ -> myTextBox.Value.BtnCommand("DeSelectAll"))
    do btnDeSelectLine.Click.Add(fun _-> myTextBox.Value.BtnCommand("DeSelectLine"))
    

    
    do btnLeftUp.Click.Add(fun _      -> myTextBox.Value.BtnCommand("LeftUp")) 
    do btnLineLeft.Click.Add(fun _    -> myTextBox.Value.BtnCommand("LineLeft")) 
    do btnLineRight.Click.Add(fun _   -> myTextBox.Value.BtnCommand("LineRight")) 
    do btnRightDown.Click.Add(fun _   -> myTextBox.Value.BtnCommand("RightDown"))
    do btnSelectAll.Click.Add(fun _   -> myTextBox.Value.BtnCommand("SelectAll"))
    do btnSelectLine.Click.Add(fun _  -> myTextBox.Value.BtnCommand("SelectLine"))

    do btnFindReplace.Click.Add(fun _ -> myTextBox.Value.BtnCommand("FindReplace"))
    do btnCopyGroup.Click.Add(fun _   -> myTextBox.Value.BtnCommand("CopyGroup"))

    do btnPlaceHolder.Click.Add(fun _  -> colorChange(ref btnPlaceHolder, Media.Brushes.DarkBlue)
                                          do btnRecSelect.Foreground <- Media.Brushes.DarkBlue
                                          do btnPenDeSelect.Foreground <- Media.Brushes.White
                                          do btnRecDeSelect.Foreground <- Media.Brushes.White
                                          do updateBooleanMyTextBox())
                                          
    do btnRecSelect.Click.Add(fun _    -> colorChange(ref btnRecSelect,  Media.Brushes.DarkBlue)
                                          do btnPlaceHolder.Foreground <- Media.Brushes.DarkBlue
                                          do btnPenDeSelect.Foreground <- Media.Brushes.White
                                          do btnRecDeSelect.Foreground <- Media.Brushes.White
                                          do updateBooleanMyTextBox())

    do btnPenDeSelect.Click.Add(fun _  -> colorChange(ref btnPenDeSelect,  Media.Brushes.White)
                                          do btnRecSelect.Foreground <- Media.Brushes.DarkBlue
                                          do btnPlaceHolder.Foreground <- Media.Brushes.DarkBlue
                                          do btnRecDeSelect.Foreground <- Media.Brushes.White
                                          do updateBooleanMyTextBox() )
    
    do btnRecDeSelect.Click.Add(fun _  -> colorChange(ref btnRecDeSelect,  Media.Brushes.White)
                                          do btnRecSelect.Foreground <- Media.Brushes.DarkBlue
                                          do btnPenDeSelect.Foreground <- Media.Brushes.White
                                          do btnPlaceHolder.Foreground <- Media.Brushes.DarkBlue
                                          do updateBooleanMyTextBox())

    
    member x.MyTextBox with set(v) =  myTextBox <- v 

    //member x.BlnGroupOperation with get() = blnColorChange(ref btnGroupOperation) 
    //member x.BlnPlaceHolder    with get() = blnColorChange(ref btnPlaceHolder) 
    //member x.BlnRecSelect      with get() = blnColorChange(ref btnRecSelect) 
    //member x.BlnPenDeSelect    with get() = blnColorChange(ref btnPenDeSelect) 
    //member x.BlnRecDeSelect    with get() = blnColorChange(ref btnRecDeSelect) 

