namespace CommonClassLibrary

open System.Windows
open System.Windows.Controls
open System.Windows.Input
open System.Windows.Media
open System.Windows.Shapes

open Utilities
open Microsoft.FSharp.Control

type  SelectedRectangle()  as this = 
    inherit  UserControl()
    // Load SelectedRectangle.xaml

    //do this.Content <- contentAsXamlObject("CaretCanvas") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("CommonClassLibrary","SelectedRectangle") // Load XAML


    let mutable myRec : Rectangle = this.Content?myRec 
    let mutable tr : TranslateTransform = this.Content?tr
 
    member x.RecW with get() = myRec.Width    and set(v) = myRec.Width  <- v
    member x.RecH with get() = myRec.Height   and set(v) = myRec.Height <- v

    member x.RecBrush  with get() = myRec.Fill  and set(v) = myRec.Fill <- v
    
    member x.TrX with get() = tr.X  and set(v) = tr.X   <- v
    member x.TrY with get() = tr.Y and set(v) = tr.Y <- v 
    member x.Opacity with get() = myRec.Opacity and set(v) = myRec.Opacity <- v

    member x.SelectedRectangle(rW, rH, trX, trY , br, opt) =
                  do myRec.Width <- rW
                  do myRec.Height <- rH
                  do tr.X <- trX
                  do tr.Y <- trY
                  do myRec.Fill <- br  
                  do myRec.Opacity <- opt

    

    
 



