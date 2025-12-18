using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GDFrameworkExtend.ResKit
{
    [Serializable]
    public class EncryptConfig
    {
        public bool EncryptAB = false;
        public bool EncryptKey = false;
        public string AESKey = "QFramework";    
    }
}

