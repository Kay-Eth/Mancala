using Godot;
using System;

namespace KayEth.Mancala.UI
{
    public abstract class StoneContainer : NinePatchRect
    {
        public static Texture StoneTexture;

        Control _stonesContainer;
        Label _stonesCountLabel;

        int _count = 0;
        public int StonesCount { get { return _count; } }

        public override void _Ready()
        {
            _stonesContainer = GetNode<Control>("StonesContainer");
            _stonesCountLabel = GetNode<Label>("Count");
        }

        public void AddStone(Color? color = null)
        {
            _count++;
            _stonesCountLabel.Text = GD.Str(_count);

            if (color == null)
                color = new Color(GD.Randf(), GD.Randf(), GD.Randf());
            
            var stone = new TextureRect();
            stone.Modulate = (Color)color;
            stone.Texture = StoneTexture;
            stone.RectPivotOffset = new Vector2(36, 24);

            _stonesContainer.AddChild(stone);
            stone.RectPosition = new Vector2(_stonesContainer.RectSize.x * GD.Randf(), _stonesContainer.RectSize.y * GD.Randf()) - new Vector2(36, 24);
            stone.RectRotation = GD.Randf() * 360;
        }

        public Color RemoveStone()
        {
            if (_count == 0)
                Logger.Fatal("Cannot remove stone from container with stone count == 0");
            
            _count--;
            _stonesCountLabel.Text = GD.Str(_count);
            var child = _stonesContainer.GetChild<TextureRect>(0);
            var color = child.Modulate;
            child.QueueFree();
            return color;
        }
    }
}
