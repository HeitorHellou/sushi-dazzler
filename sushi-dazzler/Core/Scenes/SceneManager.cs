using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SushiDazzler.Core.Scenes;

public class SceneManager
{
    private IScene _currentScene;
    private IScene _nextScene;

    public IScene Current => _currentScene;

    public void ChangeScene(IScene scene)
    {
        _nextScene = scene;
    }

    public void Update(GameTime gameTime)
    {
        if (_nextScene != null)
        {
            _currentScene?.Exit();
            _currentScene = _nextScene;
            _nextScene = null;
            _currentScene.Enter();
        }

        _currentScene?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _currentScene?.Draw(spriteBatch);
    }
}
