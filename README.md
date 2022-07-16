# LogUtil

一个简单的日志工具类，无需配置，不依赖第三方库

集成了NLog和log4net，以便于对比测试

## 结论

1. 单进程，性能和NLog差不多

2. 多进程，性能比NLog差一些

3. 单进程请使用LogUtil，多进程请使用LogMutex



