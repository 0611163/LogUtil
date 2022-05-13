# LogUtil
一个简单的日志工具类，无需配置

集成了NLog和log4net，以便于对比测试

## 结论

1. 单进程单线程，忽略掉误差，LogUtil和NLog一样快，都比log4net快一倍

2. 单进程多线程，忽略掉误差，LogUtil和NLog一样快，都比log4net快一倍

3. 多进程单线程，忽略掉误差，NLog比LogUtil快一倍，NLog跑满所有线程(4核8线程CPU跑满)，LogUtil未跑满所有线程(4核8线程CPU跑满6线程，另外两个线程未跑满)，log4net未测试

4. 多进程多线程，忽略掉误差，NLog比LogUtil快一倍，NLog跑满所有线程(任务管理器CPU占用100%，所有线程跑满)，LogUtil未跑满所有线程(任务管理器CPU占用100%，但未跑满所有线程，我自己的CPU监控工具显示CPU占用率60%)，log4net未测试。(该项测试需要稍微修改一下测试代码)



