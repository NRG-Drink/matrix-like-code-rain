using NRG.Matrix.Settings;

namespace NRG.Matrix.Styles;

public interface IMatrixStyle
{
    public Task HandleKeyInput(ConsoleKeyInfo keyInfo);
    //public Task ApplySettings(IStyleSettings settings);
    public Task<bool> UpdateInternalObjects();
    public Task DisplayFrame();
}
