﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameWork.Asset
{
    public class AssetBundleAsyncLoadProgress
    {
        public float percent;
        public int total;
        public int complete;
        public AssetBundleAsyncLoader loader;
    }
}
