namespace DragonU3DSDK.Asset
{
    [System.Serializable]
    public enum AssetState
    {
        NotExist,           //不存在
        ExistInDownLoad,    //在下载目录
    }
    
    //*****新增成员时，必须处理VersionItemInfo中的Refresh*****
    [System.Serializable]
    public struct AssetBundleInfo
    {
        // ab包名
        public string AssetBundleName;

        // 依赖列表
        public string[] DependenciesBundleNames;

        // 作为增量id
        public string HashString;

        // 通过文件数据计算的MD5码
        // 作为下载校验码
        public string Md5;

        // 校验值
        public uint Crc;

        //资源状态
        public AssetState State;
        
        // 是否来自外部
        public bool FromOutside;
        
        // 外部ab包名
        public string AssetBundleNameOutside;
        
        // 外部group名称
        public string AssetBundleGroupOutside;
        
        // 外部ab包路径
        public string AssetBundlePathOutside;
    }
}
