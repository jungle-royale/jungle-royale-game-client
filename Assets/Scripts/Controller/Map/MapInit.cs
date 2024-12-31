using UnityEngine;

public class Map
{
    float width;
    float length;

    const float MAP_HEIGHT = 4f;

    public Map(float width, float length)
    {
        this.width = width;
        this.length = length;
    }

    public Vector3 Position()
    {
        return new Vector3(this.width / 2, -(MAP_HEIGHT / 2), this.length / 2);
    }

    public Vector3 Scale()
    {
        return new Vector3(this.width, MAP_HEIGHT, this.length);
    }

    public string toString()
    {
        return $"Width = {this.width}, Height = {this.length}";
    }
}