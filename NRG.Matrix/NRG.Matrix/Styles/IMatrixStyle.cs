namespace NRG.Matrix.Styles;

public interface IMatrixStyle
{
    public void HandleKeyInput(ConsoleKeyInfo keyInfo);
    //public void ApplySettings(IStyleSettings settings);
    public bool UpdateInternalObjects();
    public void DisplayFrame();
}
