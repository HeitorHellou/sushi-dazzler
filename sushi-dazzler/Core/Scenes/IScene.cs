using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SushiDazzler.Core.Scenes;

public interface IScene
{
    void Enter();
    void Exit();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}
