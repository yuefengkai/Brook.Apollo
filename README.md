# Docker-Compose 一键部署分布式配置中心Apollo
## 简介`
说起分布式肯定要想到分布式配置中心、分布式日志、分布式链路追踪等

在分布式部署中业务往往有很多配置比如: 应用程序在启动和运行时需要读取一些配置信息，配置基本上伴随着应用程序的整个生命周期，比如：数据库连接参数、启动参数等,都需要去维护和配置,但不可能一台台服务器登录上去配置
 今天我要跟大家分享一下分布式配置中心Apollo:
>  Apollo（阿波罗）是携程框架部门研发的分布式配置中心，能够集中化管理应用不同环境、不同集群的配置，配置修改后能够实时推送到应用端，并且具备规范的权限、流程治理等特性，适用于微服务配置管理场景。

## 搭建
官方文档中有两种搭建方式一种是下载源代码进行搭建，一种是使用Docker或者K8S进行搭建，今天我们使用Docker来进行搭建,毕竟Docker对于开发者来说更友好一些。

如果已有Mysql服务，推荐已有Mysql服务或者云服务RDS来当数据库使用，毕竟数据无价。

``` 
version: "3"
services:
  apollo-configservice: #Config Service提供配置的读取、推送等功能，服务对象是Apollo客户端
    image: apolloconfig/apollo-configservice:1.8.1
    restart: always
    #container_name: apollo-configservice
    volumes:
          - ./logs/apollo-configservice:/opt/logs
    ports:
      - "8080:8080"
    environment:
      - TZ='Asia/Shanghai'    
      - SERVER_PORT=8080
      - EUREKA_INSTANCE_IP_ADDRESS=xxx.xxx.xxx.xxx
      - EUREKA_INSTANCE_HOME_PAGE_URL=http://xxx.xxx.xxx.xxx:8080
      - SPRING_DATASOURCE_URL=jdbc:mysql://xxx.xxx.xxx.xxx:3306/ApolloConfigDB?characterEncoding=utf8&serverTimezone=Asia/Shanghai
      - SPRING_DATASOURCE_USERNAME=root
      - SPRING_DATASOURCE_PASSWORD=MysqkPassWord!
      
      
  apollo-adminservice: #Admin Service提供配置的修改、发布等功能，服务对象是Apollo Portal（管理界面）
    image: apolloconfig/apollo-adminservice:1.8.1
    restart: always
    #container_name: apollo-adminservice
    volumes:
      - ./logs/apollo-adminservice:/opt/logs
    ports:
      - "8090:8090"
    depends_on:
      - apollo-configservice
    environment:
      - TZ='Asia/Shanghai'    
      - SERVER_PORT=8090
      - EUREKA_INSTANCE_IP_ADDRESS=xxx.xxx.xxx.xxx
      - SPRING_DATASOURCE_URL=jdbc:mysql://xxx.xxx.xxx.xxx:3306/ApolloConfigDB?characterEncoding=utf8&serverTimezone=Asia/Shanghai
      - SPRING_DATASOURCE_USERNAME=root
      - SPRING_DATASOURCE_PASSWORD=MysqkPassWord!
      
      
  apollo-portal: #管理界面
    image: apolloconfig/apollo-portal:1.8.1
    restart: always
    container_name: apollo-portal
    volumes:
      - ./logs/apollo-portal:/opt/logs
    ports:
      - "8070:8070"
    depends_on:
      - apollo-adminservice
    environment:
      - TZ='Asia/Shanghai'    
      - SERVER_PORT=8070
      - EUREKA_INSTANCE_IP_ADDRESS=xxx.xxx.xxx.xxx
      - APOLLO_PORTAL_ENVS=dev
      - DEV_META=http://xxx.xxx.xxx.xxx:8080
      - SPRING_DATASOURCE_URL=jdbc:mysql://xxx.xxx.xxx.xxx:3306/ApolloPortalDB?characterEncoding=utf8&serverTimezone=Asia/Shanghai
      - SPRING_DATASOURCE_USERNAME=root
      - SPRING_DATASOURCE_PASSWORD=MysqkPassWord!
```
从以上docker-compose.yaml中可以看出共包含3个服务，分别为:
1. Config Service提供配置的读取、推送等功能，服务对象是Apollo客户端
2. Admin Service提供配置的修改、发布等功能，服务对象是Apollo Portal（管理界面）
3. Portal（管理界面）
如果想了解它们之间的运行方式推荐查看[官方文档](https://www.apolloconfig.com/#/zh/design/apollo-design)

日志挂载到外部./logs目录下

大家可以看到上方并没有给出Mysql的部署，如果需要使用容器部署Mysql可以参照下方docker-compose.yaml
```
version: '3'

services: 

  mysql: # myslq 数据库
    image: 'mysql/mysql-server'
    container_name: 'mysql'
    restart: always
    command: --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci --lower-case-table-names=1
    environment: #环境变量
      MYSQL_ROOT_HOST: "%" 
      MYSQL_ROOT_PASSWORD: password
      MYSQL_USER: brook
      MYSQL_PASSWORD: password
    ports:
      - "3306:3306"

```
上述mysql的docker-compose.yaml 仅供测试使用

初始化数据库
初始化 [apolloconfigdb.sql](https://github.com/apolloconfig/apollo/blob/master/scripts/docker-quick-start/sql/apolloconfigdb.sql) 和 [apolloportaldb.sql](https://github.com/apolloconfig/apollo/blob/master/scripts/docker-quick-start/sql/apolloportaldb.sql)
![-w206](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144846571-1524897839.jpg)
数据库初始化后，记得修改apolloconfigdb库中serverconfig表的 eureka.service.url 否则 apollo-adminservice无法注册到eureka
![-w872](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144846335-545846024.jpg)

修改后切换到Apollo docker-compose.yaml目录 然后使用
```
docker-compose up -d #启动文件中的三个服务并且后台运行 
```
![-w734](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144846068-925932073.jpg)
查看启动情况
```
docker-compose ps 
```
![-w930](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144845811-1536678831.jpg)
访问 http://10.0.0.53:8070/ #Apollo管理端
![-w977](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144845567-2005012479.jpg)
默认用户名:apollo
默认密码:admin
![-w1284](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144845246-951969601.jpg)
创建一个测试项目
![-w927](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144844835-497635634.jpg)
![-w1417](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144844570-463788130.jpg)

## 测试
创建一个.NetCore项目 添加Apollo.net client 
![-w840](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144844074-467644751.jpg)

添加Apollo
![-w671](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144843639-1119121709.jpg)
配置Apollo
![-w574](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144843226-797266883.jpg)

配置如上


![-w955](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144842861-573302818.jpg)

添加测试内容
代码中获取Apollo
![-w809](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144842616-514308519.jpg)

启动程序 请求/weatherforecast/apollotest 
![-w567](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144842260-1038650472.jpg)
发现并未获取到apollo中设置的配置

检查Apollo发现配置的值并没有发布
![-w1218](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144841738-424645943.jpg)
所以大家配置或者修改了Apollo一定记得发布，我们发布后再次刷新浏览器
![-w566](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144841456-1618996785.jpg)
发现数据已经是新的数据了，我们再次修改一下Apollo的Value
![-w951](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144841108-150584178.jpg)
刷新
![-w555](https://img2020.cnblogs.com/blog/286805/202109/286805-20210913144840718-1807167954.jpg)
致此 Apollo已经搭建完毕并且可以正常使用了

## 代码
示例中的代码在
https://github.com/yuefengkai/Brook.Apollo
欢迎大家Start

注意如果程序启动后无法拉取配置,可以打开Apollo的日志,在控制台中可以看到详细的配置 放到Program.cs Main函数第一行即可！
```
LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Trace); 
```

## 参考
1.https://github.com/apolloconfig/apollo.net
2.https://github.com/apolloconfig/apollo
3.https://github.com/apolloconfig/apollo/tree/master/scripts/docker-quick-start









