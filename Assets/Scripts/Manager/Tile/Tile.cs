using UnityEngine;

public class Tile
{
    // GameStart
    // public int tileScale;

    // GameUpdate
    public string tileId;
    public float X;
    public float Y;
    public int tileType;
    const float TILE_SCALE = 1f;
    const float TILE_PIVOT = 10f;
    const float TILE_HEIGHT = 10f;

    public int warning = 0;

    public Tile(string tileId, float x, float y, int warning, int tileType)
    {
        this.tileId = tileId;
        this.X = x;
        this.Y = y;
        this.tileType = tileType;
        this.warning = warning;
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