namespace GoodFriend.Interfaces;

using System;

public interface IScreen : IDisposable
{
    void Draw();
    void Show();
    void Hide();
}