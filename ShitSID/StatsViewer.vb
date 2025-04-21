Imports System.ComponentModel
Imports System.Reflection

Public Class StatsViewer
    Function GetVariable(instance As Object, variableName As String) As Object
        Dim prop As PropertyInfo = instance.GetType().GetProperty(variableName)
        If prop IsNot Nothing Then
            Return prop.GetValue(instance)
        End If

        Dim field As FieldInfo = instance.GetType().GetField(variableName)
        If field IsNot Nothing Then
            Return field.GetValue(instance)
        End If

        Return Nothing ' Variable not found
    End Function
    Function GetVariableType(instance As Object, variableName As String) As Object
        Dim prop As PropertyInfo = instance.GetType().GetProperty(variableName)
        If prop IsNot Nothing Then
            Return prop.GetValue(instance)
        End If

        Dim field As FieldInfo = instance.GetType().GetField(variableName)
        If field IsNot Nothing Then
            Return field.GetValue(instance)
        End If

        Return Nothing ' Variable not found
    End Function

    ' Function to set a variable's value by name
    Sub SetVariable(instance As Object, variableName As String, value As String)
        Dim prop As PropertyInfo = instance.GetType().GetProperty(variableName)
        If prop IsNot Nothing AndAlso prop.CanWrite Then
            Dim convertedValue As Object = ConvertStringToType(value, prop.PropertyType)
            prop.SetValue(instance, convertedValue)
            Exit Sub
        End If

        Dim field As FieldInfo = instance.GetType().GetField(variableName)
        If field IsNot Nothing Then
            Dim convertedValue As Object = ConvertStringToType(value, field.FieldType)
            field.SetValue(instance, convertedValue)
        End If
    End Sub
    Function GetAllVariableNames(instance As Object) As List(Of String)
        Dim names As New List(Of String)()

        ' Get all public properties
        Dim properties As PropertyInfo() = instance.GetType().GetProperties()
        For Each prop In properties
            names.Add(prop.Name)
        Next

        ' Get all public fields
        Dim fields As FieldInfo() = instance.GetType().GetFields()
        For Each field In fields
            names.Add(field.Name)
        Next

        Return names
    End Function
    Function ConvertStringToType(value As String, targetType As Type) As Object
        Try
            Dim converter As TypeConverter = TypeDescriptor.GetConverter(targetType)
            If converter IsNot Nothing AndAlso converter.CanConvertFrom(GetType(String)) Then
                Return converter.ConvertFromString(value)
            End If
        Catch ex As Exception
            Console.WriteLine($"Error converting '{value}' to {targetType.Name}: {ex.Message}")
        End Try
        Return Nothing
    End Function

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        'GLOBAL
        Dim outG = ""
        For Each x As String In GetAllVariableNames(Form1.sid)
            outG += $"{x.PadRight(20)} = {GetVariable(Form1.sid, x)}" & vbCrLf
        Next
        Label1.Text = outG

        ' Voice 1
        Dim out1 = ""
        For Each x As String In GetAllVariableNames(Form1.sid.Voices(0))
            out1 += $"{x.PadRight(20)} = {GetVariable(Form1.sid.Voices(0), x)}" & vbCrLf
        Next
        Label2.Text = out1

        ' Voice 2
        Dim out2 = ""
        For Each x As String In GetAllVariableNames(Form1.sid.Voices(1))
            out2 += $"{x.PadRight(20)} = {GetVariable(Form1.sid.Voices(1), x)}" & vbCrLf
        Next
        Label3.Text = out2

        ' Voice 3
        Dim out3 = ""
        For Each x As String In GetAllVariableNames(Form1.sid.Voices(2))
            out3 += $"{x.PadRight(20)} = {GetVariable(Form1.sid.Voices(2), x)}" & vbCrLf
        Next
        Label4.Text = out3
    End Sub
End Class