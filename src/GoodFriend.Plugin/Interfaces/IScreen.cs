namespace GoodFriend.Interfaces;

using System;

/// <summary> Specifies the implementation necessary for a screen </summary>
public interface IScreen : IDisposable
{
    void Draw();
    void Show();
    void Hide();
}