# LogUtil
一个简单的日志工具类，无需配置，不依赖第三方库

集成了NLog和log4net，以便于对比测试

## 结论

1. 单进程，不论是单线程还是多线程，忽略掉误差，LogUtil和NLog一样快，都比log4net快一倍

2. 多进程，NLog比LogUtilUseMutex快(测试中发现NLog对CPU线程的利用率高，LogUtilUseMutex对CPU线程的利用率低)，log4net未测试

3. 综上，单进程程序，建议使用LogUtil，多进程程序建议使用LogUtilUseMutex



