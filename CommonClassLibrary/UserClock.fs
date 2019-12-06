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
open  System.Threading
open System.IO
open System.Windows.Markup
open System.Xaml
open System.Reflection
open System.Windows.Media
open Utilities



open Microsoft.FSharp.Control

type  UserClock()  as this = 
    inherit  UserControl()

    //do this.Content <- contentAsXamlObject("UserClock") // Load XAML
    do this.Content <- contentAsXamlObjectFromAssembly("CommonClassLibrary","UserClock") // Load XAML


    let mutable viewBox : Viewbox = this.Content?viewBox
    let mutable pathData : System.Windows.Shapes.Path = this.Content?pathData
    let mutable pathArrow : System.Windows.Shapes.Path = this.Content?pathArrow

    do pathData.Fill <- new SolidColorBrush(Colors.LightGray)

    member x.PathDataColor with  set(v) = pathData.Fill <- new SolidColorBrush(v)  