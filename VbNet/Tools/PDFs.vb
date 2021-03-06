Option Infer Off
Option Explicit On
Option Strict On
Imports System
Imports System.IO
Imports System.Drawing
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports Microsoft.VisualBasic
Imports System.Drawing.Imaging
Public Class PDFs
#Region "Left"
    Public Shared Function Left(ByVal Cadena As String, ByVal Posiciones As Integer) As String
        If Posiciones > Cadena.Trim.Length Then Return Cadena
        Return Cadena.Trim.Substring(0, Posiciones)
    End Function
#End Region
#Region "Right"
    Public Shared Function Right(ByVal Cadena As String, ByVal Posiciones As Integer) As String
        If Posiciones > Cadena.Trim.Length Then Return Cadena
        Return Cadena.Trim.Substring(Cadena.Trim.Length - Posiciones, Posiciones)
    End Function
#End Region
#Region "TemporaryFileName"
    Public Shared Function TemporaryFileName(ByVal Extension As String) As String
        Dim r As Random = New Random
        Dim NumeroRandom As String = r.Next(1, 999999).ToString
        TemporaryFileName = Date.Today.Year.ToString & Date.Today.Month.ToString & Date.Today.Day.ToString & Date.Today.Minute.ToString & Date.Today.Second.ToString & NumeroRandom.ToString & "." & Extension
    End Function
#End Region
#Region "GiveMeFileName"
    Public Shared Function GiveMeFileName(ByVal ThePath As String) As String
        Dim N As Integer
        Dim TheCharacter As String = ""
        For N = ThePath.Trim.Length To 1 Step -1
            TheCharacter = Right(Left(ThePath, N), 1)
            If TheCharacter = "/" Or TheCharacter = "\" Then
                TheCharacter = Right(ThePath, ThePath.Trim.Length - N)
                Exit For
            End If
        Next
        GiveMeFileName = TheCharacter
    End Function
#End Region
#Region "WaterMarkInTif"
    Public Function WaterMarkInTif(ByVal InputFile As String, ByVal OutputFile As String, ByVal WaterMarkText As String) As String
        WaterMarkInTif = "OK"
        Try
            Dim OTIFF As New TiffUtil(System.IO.Path.GetTempPath())
            If OTIFF.getPageCount(InputFile) > 1 Then
                Dim WorkingDirectory As String = System.IO.Path.GetTempPath & System.DateTime.Now.Ticks.ToString
                If Not IO.Directory.Exists(WorkingDirectory) Then IO.Directory.CreateDirectory(WorkingDirectory)
                OTIFF.SplitTiffPages(InputFile, WorkingDirectory)
                Dim Files() As String
                Files = IO.Directory.GetFiles(WorkingDirectory, "*.tif")
                Dim n As Integer
                For n = 0 To Files.Length - 1
                    Dim ImageFile As System.IO.StreamReader = New System.IO.StreamReader(Files(n))
                    Dim Bm As New System.Drawing.Bitmap(ImageFile.BaseStream)
                    Dim Tmp As New Bitmap(Bm)
                    Dim canvas As Graphics = Graphics.FromImage(Tmp)
                    canvas.DrawImage(Bm, New System.Drawing.Rectangle(0, 0, Tmp.Width, Tmp.Height), 0, 0, Tmp.Width, Tmp.Height, GraphicsUnit.Pixel)
                    canvas.DrawString(WaterMarkText.Trim, New System.Drawing.Font("Verdana", 56, FontStyle.Bold), New SolidBrush(System.Drawing.Color.Red), CSng((Tmp.Width / 2) - ((WaterMarkText.Trim.Length * 40) / 2)), 0)
                    ImageFile.Close()
                    ImageFile.Dispose()
                    canvas = Nothing
                    Tmp.Save(WorkingDirectory & "\WM__" & GiveMeFileName(Files(n)))
                    Tmp = Nothing
                    Bm = Nothing
                Next
                Files = IO.Directory.GetFiles(WorkingDirectory, "WM__*.tif")
                OTIFF.mergeTiffPages(Files, WorkingDirectory & "\FINAL_.tif")
                IO.File.Copy(WorkingDirectory & "\FINAL_.tif", OutputFile)
                Files = Nothing
            Else
                Dim ImageFile As System.IO.StreamReader = New System.IO.StreamReader(InputFile)
                Dim Bm As New System.Drawing.Bitmap(ImageFile.BaseStream)
                Dim Tmp As New Bitmap(Bm)
                Dim canvas As Graphics = Graphics.FromImage(Tmp)
                canvas.DrawImage(Bm, New System.Drawing.Rectangle(0, 0, Tmp.Width, Tmp.Height), 0, 0, Tmp.Width, Tmp.Height, GraphicsUnit.Pixel)
                canvas.DrawString(WaterMarkText.Trim, New System.Drawing.Font("Verdana", 56, FontStyle.Bold), New SolidBrush(System.Drawing.Color.Red), CSng((Tmp.Width / 2) - ((WaterMarkText.Trim.Length * 40) / 2)), 0)
                canvas = Nothing
                ImageFile.Close()
                ImageFile.Dispose()
                Tmp.Save(OutputFile)
                Tmp = Nothing
                Bm = Nothing
            End If
        Catch ex As Exception
            WaterMarkInTif = ex.Message
        End Try
    End Function
#End Region
#Region "TxtToPdf"
    Public Function TxtToPdf(ByVal TxtFile As String, ByVal OutPutFile As String) As String
        TxtToPdf = "OK"
        Try
            Dim oDoc As New iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 0, 0, 0, 0)
            Dim pdfw As iTextSharp.text.pdf.PdfWriter
            Dim cb As iTextSharp.text.pdf.PdfContentByte
            pdfw = iTextSharp.text.pdf.PdfWriter.GetInstance(oDoc, New FileStream(OutPutFile, IO.FileMode.Create, FileAccess.Write, IO.FileShare.None))
            'Apertura del documento.
            oDoc.Open()
            'Agregamos una pagina.
            oDoc.NewPage()
            'Iniciamos el flujo de bytes.
            Dim MyIOReader As New StreamReader(TxtFile)
            Dim MyLine As String
            MyLine = MyIOReader.ReadLine
            Dim Y As Single = iTextSharp.text.PageSize.A4.Height - 50
            cb = pdfw.DirectContent
            Do While Not MyLine Is Nothing
                Dim vFont As New iTextSharp.text.Font(iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL).BaseFont)
                oDoc.Add(New Paragraph(MyLine, vFont))
                'cb.BeginText()
                'Instanciamos el objeto para el tipo de letra.
                'MyFont = iTextSharp.text.FontFactory.GetFont(iTextSharp.text.FontFactory.HELVETICA, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL).BaseFont
                'Seteamos el tipo de letra y el tamaño.
                'cb.SetFontAndSize(MyFont, 12)
                'Seteamos el color del texto a escribir.
                'cb.SetColorFill(iTextSharp.text.Color.BLACK)
                'Aqui es donde se escribe el texto.
                'Aclaracion: Por alguna razon la coordenada vertical siempre es tomada desde el borde inferior (de ahi que se calcule como "PageSize.A4.Height - 50″)
                'cb.ShowTextAligned(iTextSharp.text.pdf.PdfContentByte.ALIGN_CENTER, MyLine, 0, Y, 0)
                'Y = Y + 50
                'Fin del flujo de bytes.
                'cb.EndText()
                MyLine = MyIOReader.ReadLine
            Loop
            MyIOReader.Close()
            MyIOReader.Dispose()
            'Forzamos vaciamiento del buffer.
            pdfw.Flush()
            'Cerramos el documento.
            oDoc.Close()
            cb = Nothing
            pdfw = Nothing
            oDoc = Nothing
        Catch ex As Exception
            TxtToPdf = ex.Message
        End Try
    End Function
#End Region
#Region "TifAPdf"
    Public Function TIFToPdf(ByVal TiffFile As String, ByVal PdfFile As String) As String
        Try
            TIFToPdf = "OK"
            Dim Bm As New System.Drawing.Bitmap(TiffFile)
            Dim Document As Document
            Dim Escala As Single = 0
            Dim TiffApaisado As Boolean = (Bm.Width > Bm.Height)
            Select Case True
                Case TiffApaisado
                    Dim Pagesize2 As New iTextSharp.text.Rectangle(2000, 1250)
                    Document = New Document(Pagesize2, 50, 50, 50, 50)
                    Escala = 55
                Case Bm.Height >= 2879
                    Dim Pagesize2 As New iTextSharp.text.Rectangle(1250, 2000)
                    Document = New Document(Pagesize2, 50, 50, 50, 50)
                    Escala = 70
                Case Else
                    Document = New Document(PageSize.A4, 50, 50, 50, 50)
                    Escala = 72.0F / 200.0F * 100
            End Select
            Dim Writer As PdfWriter = PdfWriter.GetInstance(Document, New FileStream(PdfFile, FileMode.Create))
            Dim Total As Integer = Bm.GetFrameCount(Drawing.Imaging.FrameDimension.Page)
            Document.Open()
            Dim cb As PdfContentByte = Writer.DirectContent
            For k As Integer = 0 To Total - 1
                Bm.SelectActiveFrame(Drawing.Imaging.FrameDimension.Page, k)
                Dim img As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(Bm, System.Drawing.Imaging.ImageFormat.Png)
                img.ScalePercent(Escala)
                img.SetAbsolutePosition(0, 0)
                cb.AddImage(img)
                Document.NewPage()
            Next
            Document.Close()
        Catch ex As Exception
            TIFToPdf = ex.Message
        End Try
    End Function
#End Region
#Region "ConcatFiles"
    Public Function ConcatFiles(ByVal Args() As String) As String
        ConcatFiles = "OK"
        Try
            If Args.Length < 3 Then
                Throw New System.Exception("Se requieren como minimo 3 archivos ( Destino , Archivo1, Archivo2....,Archivo100")
            End If
            Dim f As Integer = 1
            'we create a reader for a certain document
            Dim reader As New pdf.PdfReader(Args(f))
            'we retrieve the total number of pages
            Dim n As Integer = reader.NumberOfPages
            'step 1: creation of a document-object
            Dim document As New Document(reader.GetPageSizeWithRotation(1))
            'step 2: we create a writer that listens to the document
            Dim writer As pdf.PdfWriter
            writer = PdfWriter.GetInstance(document, New FileStream(Args(0), FileMode.Create))
            'step 3: we open the document
            document.Open()
            Dim cb As PdfContentByte
            cb = writer.DirectContent
            Dim page As PdfImportedPage
            Dim rotation As Integer
            'step 4: we add content
            While (f < Args.Length)
                Dim i As Integer = 0
                While i < n
                    i = i + 1
                    document.SetPageSize(reader.GetPageSizeWithRotation(i))
                    document.NewPage()
                    page = writer.GetImportedPage(reader, i)
                    rotation = reader.GetPageRotation(i)
                    If (rotation = 90 Or rotation = 270) Then
                        cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height)
                    Else
                        cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0)
                    End If
                End While
                f = f + 1
                If (f < Args.Length) Then
                    reader = New PdfReader(Args(f))
                    n = reader.NumberOfPages
                End If
            End While
            document.Close()
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function
#End Region
#Region "AddWatermarkImage"
    Public Sub AddWatermarkImage(ByVal sourceFile As String, ByVal outputFile As String, ByVal watermarkImage As String)
        Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
        Dim stamper As iTextSharp.text.pdf.PdfStamper = Nothing
        Dim img As iTextSharp.text.Image = Nothing
        Dim underContent As iTextSharp.text.pdf.PdfContentByte = Nothing
        Dim rect As iTextSharp.text.Rectangle = Nothing
        Dim X, Y As Single
        Dim pageCount As Integer = 0
        Try
            reader = New iTextSharp.text.pdf.PdfReader(sourceFile)
            rect = reader.GetPageSizeWithRotation(1)
            stamper = New iTextSharp.text.pdf.PdfStamper(reader, New System.IO.FileStream(outputFile, IO.FileMode.Create))
            img = iTextSharp.text.Image.GetInstance(watermarkImage)
            If img.Width > rect.Width OrElse img.Height > rect.Height Then
                img.ScaleToFit(rect.Width, rect.Height)
                X = (rect.Width - img.ScaledWidth) / 2
                Y = (rect.Height - img.ScaledHeight) / 2
            Else
                X = (rect.Width - img.Width) / 2
                Y = (rect.Height - img.Height) / 2
            End If
            img.SetAbsolutePosition(X, Y)
            pageCount = reader.NumberOfPages()
            For i As Integer = 1 To pageCount
                underContent = stamper.GetUnderContent(i)
                underContent.AddImage(img)
            Next
            stamper.Close()
            reader.Close()
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
#End Region
#Region "AddWatermarkText"
#Disable Warning BC40028 ' Type of parameter 'watermarkFont' is not CLS-compliant.
#Disable Warning BC40028 ' Type of parameter 'watermarkFontColor' is not CLS-compliant.
    Public Function AddWatermarkText(ByVal sourceFile As String, ByVal outputFile As String, ByVal watermarkText As String, Optional ByVal watermarkFont As iTextSharp.text.pdf.BaseFont = Nothing, Optional ByVal watermarkFontSize As Single = 48, Optional ByVal watermarkFontColor As iTextSharp.text.BaseColor = Nothing, Optional ByVal watermarkFontOpacity As Single = 0.3F, Optional ByVal watermarkRotation As Single = 45.0F) As String
#Enable Warning BC40028 ' Type of parameter 'watermarkFontColor' is not CLS-compliant.
#Enable Warning BC40028 ' Type of parameter 'watermarkFont' is not CLS-compliant.
        Try
            AddWatermarkText = "OK"
            Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
            Dim stamper As iTextSharp.text.pdf.PdfStamper = Nothing
            Dim gstate As iTextSharp.text.pdf.PdfGState = Nothing
            Dim underContent As iTextSharp.text.pdf.PdfContentByte = Nothing
            Dim rect As iTextSharp.text.Rectangle = Nothing
            Dim pageCount As Integer = 0
            reader = New iTextSharp.text.pdf.PdfReader(sourceFile)
            rect = reader.GetPageSizeWithRotation(1)
            stamper = New iTextSharp.text.pdf.PdfStamper(reader, New System.IO.FileStream(outputFile, IO.FileMode.Create))
            If watermarkFont Is Nothing Then
                watermarkFont = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.TIMES_BOLD, iTextSharp.text.pdf.BaseFont.CP1250, True) 'iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED 
            End If
            If watermarkFontColor Is Nothing Then
                watermarkFontColor = iTextSharp.text.BaseColor.BLUE
            End If
            gstate = New iTextSharp.text.pdf.PdfGState()
            gstate.FillOpacity = watermarkFontOpacity
            gstate.StrokeOpacity = watermarkFontOpacity
            pageCount = reader.NumberOfPages()
            For i As Integer = 1 To pageCount
                underContent = stamper.GetUnderContent(i)
                With underContent
                    .SaveState()
                    .SetGState(gstate)
                    .SetColorFill(watermarkFontColor)
                    .BeginText()
                    .SetFontAndSize(watermarkFont, watermarkFontSize)
                    .SetTextMatrix(30, 30)
                    .ShowTextAligned(iTextSharp.text.Element.ALIGN_CENTER, watermarkText, rect.Width / 2, rect.Height / 2, watermarkRotation)
                    .EndText()
                    .RestoreState()
                End With
            Next
            stamper.FormFlattening = True
            stamper.Close()
            reader.Close()
        Catch ex As Exception
            AddWatermarkText = ex.Message
        End Try
    End Function
#End Region
#Region "ExtractImages"
    Public Sub ExtractImages(ByVal sourcePdf As String, ByVal Path As String)
        Dim raf As iTextSharp.text.pdf.RandomAccessFileOrArray = Nothing
        Dim reader As iTextSharp.text.pdf.PdfReader = Nothing
        Dim pdfObj As iTextSharp.text.pdf.PdfObject = Nothing
        Dim pdfStrem As iTextSharp.text.pdf.PdfStream = Nothing
        Try
            raf = New iTextSharp.text.pdf.RandomAccessFileOrArray(sourcePdf)
            reader = New iTextSharp.text.pdf.PdfReader(raf, Nothing)
            Dim Counter As Integer = 0
            For i As Integer = 0 To reader.XrefSize - 1
                pdfObj = reader.GetPdfObject(i)
                If Not IsNothing(pdfObj) AndAlso pdfObj.IsStream() Then
                    pdfStrem = DirectCast(pdfObj, iTextSharp.text.pdf.PdfStream)
                    Dim subtype As iTextSharp.text.pdf.PdfObject = pdfStrem.Get(iTextSharp.text.pdf.PdfName.SUBTYPE)
                    If Not IsNothing(subtype) AndAlso subtype.ToString = iTextSharp.text.pdf.PdfName.IMAGE.ToString Then
                        Dim bytes() As Byte = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw(CType(pdfStrem, iTextSharp.text.pdf.PRStream))
                        If Not IsNothing(bytes) Then
                            Try
                                Using memStream As New System.IO.MemoryStream(bytes)
                                    memStream.Position = 0
                                    Dim img As System.Drawing.Image = System.Drawing.Image.FromStream(memStream)
                                    Counter = Counter + 1
                                    img.Save(Path & "\" & Counter.ToString.Trim & ".jpg")
                                End Using
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                End If
            Next
            reader.Close()
        Catch ex As Exception
        End Try
    End Sub
#End Region
#Region "ParsePdfText"
    Public Function ParsePdfText(ByVal sourcePDF As String, Optional ByVal fromPageNum As Integer = 0, Optional ByVal toPageNum As Integer = 0) As String
        Dim sb As New System.Text.StringBuilder()
        Try
            Dim reader As New PdfReader(sourcePDF)
            Dim pageBytes() As Byte = Nothing
            Dim token As PRTokeniser = Nothing
            Dim tknType As Integer = -1
            Dim tknValue As String = String.Empty
            If fromPageNum = 0 Then
                fromPageNum = 1
            End If
            If toPageNum = 0 Then toPageNum = reader.NumberOfPages
            If fromPageNum > toPageNum Then
                Throw New ApplicationException("Parameter error: The value of fromPageNum can " & _
                                           "not be larger than the value of toPageNum")
            End If
            For i As Integer = fromPageNum To toPageNum Step 1
                pageBytes = reader.GetPageContent(i)
                If Not IsNothing(pageBytes) Then
                    token = New PRTokeniser(pageBytes)
                    While token.NextToken()
                        tknType = token.TokenType()
                        tknValue = token.StringValue
                        If tknType = PRTokeniser.TokType.STRING Then
                            sb.Append(token.StringValue)
                            'I need to add these additional tests to properly add whitespace to the output string
                        ElseIf tknType = 1 AndAlso tknValue = "-600" Then
                            sb.Append(" ")
                        ElseIf tknType = 10 AndAlso tknValue = "TJ" Then
                            sb.Append(" ")
                        End If
                    End While
                End If
            Next i
            Return sb.ToString()
        Catch ex As Exception
            ParsePdfText = "ERROR " & ex.Message
        End Try
    End Function
#End Region
End Class
