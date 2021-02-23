using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Infos:
// http://wiki.unity3d.com/index.php?title=OffsetVanishingPoint




public static class CameraOps
{



    // ----------------------------- public functions ----------------------------



    public static void PanCamera()
    {
        Camera[] cameras = new Camera[Camera.allCamerasCount];
        Camera.GetAllCameras(cameras);
        var offset = VisualController.inst.cameraOffset;

        foreach (Camera cam in cameras)
        {
            cam.ResetProjectionMatrix();        // damit bei sich änderndem aspect ratio weiterhin alles korrekt
            SetVanishingPoint(cam, offset);     // main function
        }
    }



    // ----------------------------- private functions ----------------------------



    private static void SetVanishingPoint (Camera cam, Vector2 offset)
    {
        var matrix = cam.projectionMatrix;
        var width = 2 * cam.nearClipPlane / matrix.m00;
        var height = 2 * cam.nearClipPlane / matrix.m11;

        var left = -width / 2 - offset.x;
        var right = left + width;
        var bottom = -height / 2 - offset.y;
        var top = bottom + height;

        cam.projectionMatrix = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
    }

    private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        var x = (2f * near) / (right - left);
        var y = (2f * near) / (top - bottom);
        var a = (right + left) / (right - left);
        var b = (top + bottom) / (top - bottom);
        var c = -(far + near) / (far - near);
        var d = -(2f * far * near) / (far - near);
        var e = -1f;

        var m = new Matrix4x4();
        m[0, 0] = x; m[0, 1] = 0; m[0, 2] = a; m[0, 3] = 0;
        m[1, 0] = 0; m[1, 1] = y; m[1, 2] = b; m[1, 3] = 0;
        m[2, 0] = 0; m[2, 1] = 0; m[2, 2] = c; m[2, 3] = d;
        m[3, 0] = 0; m[3, 1] = 0; m[3, 2] = e; m[3, 3] = 0;

        return m;
    }
}
