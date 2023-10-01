using Microsoft.Xna.Framework.Input;

namespace LD54;

public class Input
{
    private KeyboardState? _lastKeyboardState;
    private KeyboardState? _keyboardState;

    public void Update(KeyboardState keyboardState)
    {
        _lastKeyboardState = _keyboardState;
        _keyboardState = keyboardState;
    }

    public bool WasKeyPressed(Keys key)
    {
        return (_keyboardState?.IsKeyDown(key) ?? false) && (!_lastKeyboardState?.IsKeyDown(key) ?? false);
    }

    public bool IsKeyHeld(Keys key)
    {
        return _keyboardState?.IsKeyDown(key) ?? false;
    }
}