# TcpDoubleProxy


If you want to have access to your home reources and you have one VM accesable via internet - that service can just help you to have access to home resources anywhere via internet.

The best case scenario.


1. Run Server Part on VM accessable by internet (linux VM);
2. Close all ports on accessable by internet VM except 22 (SSH) which has to be protected by Certificate;
3. Open Technical Port accessable from home internet ip;
3. Run ClientPart on home Laptop behind the internet;
4. Make sure that ClientPart and Server Part are connected;
4. Run ssh in portforward mode;
5. Have access to the home resource which is behind firewall;


![alt text](https://raw.githubusercontent.com/amigin/TcpDoubleProxy/master/DoubleProxy.png)

