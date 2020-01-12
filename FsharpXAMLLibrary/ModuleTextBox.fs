
module internal FsharpXAMLLibrary.ModuleTextBox

    open System.Windows
    open System.Windows.Media

    open CommonClassLibrary

    //Function "Set Caret" to Absolute Position   

    let set_Caret(crt : CaretCanvas, openUpdateMMF:OpenUpdateMMF, myFonts: Fonts, blnInsert: bool) = 
        let iChar = crt.AbsoluteNumCharCurrent - openUpdateMMF.IntFirstCharOnPage
        let iLine = crt.AbsoluteNumLineCurrent - openUpdateMMF.IntFirstLineOnPage
   
        do crt.TranslateTransform.X <- myFonts.Tb_FontSize * (float)iChar / myFonts.CoeffFont_Widh - 1.0
        do crt.TranslateTransform.Y <- myFonts.Tb_FontSize * (float)iLine / myFonts.CoeffFont_High       
        do crt.BackGroundColorCarete <- new SolidColorBrush(Colors.Magenta)

        do crt.FloatCareteH <- myFonts.Tb_FontSize / myFonts.CoeffFont_High       
        do crt.FloatCareteW <- match blnInsert with
                                | true  -> myFonts.Tb_FontSize / myFonts.CoeffFont_Widh 
                                | false -> myFonts.Tb_FontSize / myFonts.CoeffFont_Widh / 3.0
  
        let blnOut =  iLine < 0 || iChar < 0 || 
                        iLine >= (openUpdateMMF.IntVertCountLinesOnPage) || 
                        iChar >= (openUpdateMMF.IntHorizCountCharsOnPage)

        if blnOut then crt.Visibility <- Visibility.Hidden
                    else crt.Visibility <- Visibility.Visible
    ()