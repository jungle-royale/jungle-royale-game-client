using UnityEngine;

public class MapInit {
    float width;
    float length;

    public MapInit(float width, float length)
    {
        this.width = width;
        this.length = length;
    }

    public Vector3 Position()
    {
        return new Vector3(this.width / 2, 0, this.length / 2);
    }

    public Vector3 Scale()
    {
        return new Vector3(this.width, 4, this.length);
    }

    public string toString() 
    {
        return $"Width = {this.width}, Height = {this.length}";
    }
}