using UnityEngine;

namespace IVE
{
    public class ExUpdate : MonoBehaviour
    {
        private static ExUpdate _normalIns = null;
        private static ExUpdate normalIns
        {
            get
            {
                if (_normalIns == null)
                {
                    GameObject g = new GameObject("Normal ExUpdate Instance");
                    _normalIns = g.AddComponent<ExUpdate>();
                }

                return _normalIns;
            }
        }

        private static ExUpdate _dontDestroyIns = null;

        private static ExUpdate dontDestroyIns
        {
            get
            {
                if (_dontDestroyIns == null)
                {
                    GameObject g = new GameObject("Dont Destroy ExUpdate Instance");
                    _dontDestroyIns = g.AddComponent<ExUpdate>();
                }

                return _dontDestroyIns;
            }
        }


        
        public static void AddAU()
        {
            
        }
    }
}
