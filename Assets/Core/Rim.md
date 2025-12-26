环世界代码架构解析

以XML为配置基底

### Map

#### 继承的类和接口

* IIncidentTarget:根据玩家地图的财富和故事叙述者的难度来生成随机的事件
* ILoadReferenceable:当一个类或者对象是可引用的,那么其必定会有唯一ID作为其标识
* IThingHolder:获取所持有的实体(//TD 地图继承这个原因存疑)
* IExposable:初始化数据
* IDisposable:卸载数据

##### 内部重点方法和引用类

* MapFileCompressor: 地图文件压缩器(//TODO 待继续剖析)
* MapGeneratorDef:地图生成配置
* 

### Def (配置基类) : Editable
    其中defName -> 每个Def使用的名字,每个Def的Id取自defName的hash值
