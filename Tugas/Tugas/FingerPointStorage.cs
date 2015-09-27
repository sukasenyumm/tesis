
namespace Tugas
{
    public class FingerPointStorage
    {
        public float g_X, g_Y, g_Z;
        public bool numHand;
        public bool isActive;
        public float g_Pinch;
        public float g_circle;

        public FingerPointStorage(float x, float y, float z, bool num_hand,float pinch,float circleGesture)
        {
            g_X = x;
            g_Y = y;
            g_Z = z;
            g_Pinch = pinch;
            isActive = true;
            g_circle = circleGesture;
            numHand = num_hand;
        }
    }
}
