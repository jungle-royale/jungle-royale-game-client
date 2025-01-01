using UnityEngine;

public class Tile
{
    // GameStart
    // public int tileScale;

    // GameUpdate
    public int tileId;
    public float X;
    public float Y;
    const float TILE_SCALE = 20f;
    const float TILE_PIVOT = TILE_SCALE / 2;
    const float TILE_HEIGHT = 10f;

    public Tile(int tileId, float x, float y)
    {
        this.tileId = tileId;
        this.X = x;
        this.Y = y;
    }

    public Vector3 Position()
    {
        return new Vector3(this.X + TILE_PIVOT, -(TILE_HEIGHT / 2), this.Y + TILE_PIVOT);
    }

    public Vector3 Scale()
    {
        return new Vector3(TILE_SCALE, TILE_HEIGHT, TILE_SCALE);
    }

    public override string ToString()
    {
        return $"Tile Scale: {TILE_SCALE}, Position: {this.X}, {this.Y}";
    }
}