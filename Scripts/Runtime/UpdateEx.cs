using UnityEngine;

namespace Ive.Tools
{
    public class UpdateEx : MonoBehaviour
    {
        private static UpdateEx _singleton = null;
        public static UpdateEx Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    GameObject g = new GameObject("UpdateEx");
                    _singleton = g.AddComponent<UpdateEx>();
                }

                return _singleton;
            }
        }


        public static void AddAU()
        {
            
        }
    }
}
