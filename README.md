# LogUtil

一个简单的日志工具类，无需配置，不依赖第三方库

集成了NLog和log4net，以便于对比测试

## 性能

1. 单进程，Windows下性能和NLog差不多，Linux下性能比NLog好

2. 多进程，Windows下性能不如NLog，Linux下性能和NLog差不多

## 如何使用

### NuGet

[https://www.nuget.org/packages/LogUtil](https://www.nuget.org/packages/LogUtil)

### 单进程

```C#
LogUtil.Info("Info日志");
LogUtil.Debug("Debug日志");
LogUtil.Error("Error日志");
LogUtil.Error(ex, "Error日志");
LogUtil.Error("Error日志", ex);
```

### 多进程

```C#
//使用前请设置SupportMultiProcess为true，即支持多进程
LogUtil.SupportMultiProcess = true;
```

```C#
LogUtil.Info("Info日志");
LogUtil.Debug("Debug日志");
LogUtil.Error("Error日志");
LogUtil.Error(ex, "Error日志");
LogUtil.Error("Error日志", ex);
```
