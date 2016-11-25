using UnityEngine;

public static class RendererExtensions
{
  private static Plane[] planes;
  private static int frame = -1;

  public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
  {
    if(frame != Time.frameCount)
    {
      planes = GeometryUtility.CalculateFrustumPlanes(camera);
      frame = Time.frameCount;
    }
    
    return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
  }
}