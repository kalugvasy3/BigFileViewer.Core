namespace FsharpXAMLLibrarySupport

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

open CommonClassLibrary
open FsharpXAMLLibrary 
open Utilities

open Microsoft.FSharp.Control

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

    do this.Content <-   contentAsXamlObjectFromAssembly("FsharpXAMLLibrarySupport","QuickFind") // Load XAML
 
    let mutable myTextBox = new MyTextBox()
    let mutable openUpdateMMF = new OpenUpdateMMF() 

    
    let mutable txtQuickFind : TextBox = this.Content?txtQuickFind 
    let mutable btnFindAll : Button = this.Content?btnFindAll
    let mutable btnFindNext : Button = this.Content?btnFindNext
    let mutable stackPanel : StackPanel = this.Content?stackPanel

    let findInText (x : int*int) = let (iLine, iChar) = x
                                   do myTextBox.IntFirstLineOnPage <- iLine
                                   do myTextBox.IntFirstCharOnPage <- iChar

    let addNewPosition(x: int*int , str: string) = 
                                   let mutable txt = new MyTextBlock()
                                   txt.Position <- x
                                   txt.Text <- x.ToString() + "  " + str
                                   txt.MouseLeftButtonDown.Add(fun _ -> findInText(txt.Position)) 
                                   stackPanel.Children.Add(txt)

    let findNext() = ignore()
    let findAll() = ignore()




    do txtQuickFind.TextChanged.Add(fun _ -> stackPanel.Children.Clear() )                                     
    do btnFindAll.Click.Add(fun _ -> findAll())                                
    do btnFindNext.Click.Add(fun _ -> findNext())  
    
    member x.InitMyTextBox(txt : MyTextBox byref) = do myTextBox <- txt
                                                    do openUpdateMMF <- txt.OpenUpdateMMF
  
                                                            
  
 

    

    
 



