# netsnake

This utility allows you to capture network packets on the selected network interface. A chart o the volume of transmitted and received packets is displayed. It has an ability to filter intercepted packets by source ip,source port, destination ip,destination port, type of protocol and length packets.
It is possible to save a dump of captured packets in .pcap file extension for opening by WireShark software. Also,the main advantage of netsnake is sending your own network packets (ARP,TCP/IP,UDP) whith chosen parameters including payload data.which is byte array with max length = 1450 bytes. If your array of bytes has more 1450 bytes, it breaks into packages.
In this project for capture packets used opensource library sharppcap https://github.com/chmorgan/sharppcap

---------------------------------------
----------Filter options---------------
---------------------------------------
This filter can be used only with stopped capture of packets!!!

--------------------------------------------------------------------------------
1. KeyWords:
--------------------------------------------------------------------------------
 -ipsource, filtered by IP Source ;
 -ipdest, filtered by IP Destination ;
 -portsource, filtered by Port Source ;
 -portdest, filtered by Port Destination ;
 -prot, filtered by type of Protocol(IP,TCP,UDP,ARP,ICMPV6,IGMP and other);
 -length, filtered by length value of packets.


---------------------------------------------------------------------------------
2. Logical operations:
---------------------------------------------------------------------------------
& -operand 'and', which union 2 and more parametrs;
| -operand 'or', which disjunction 2 and more parametrs.


----------------------------------------------------------------------------------------------------------------------
3. Examples:
----------------------------------------------------------------------------------------------------------------------
Exmle one:
Filter captured packets by IP Source = 192.168.0.1 and Port destination = 443 and protocol type = UDP and length = 1400.
Command: ipsource=192.168.0.1 & portdest=443 & prot=UDP & length=1400

Exmle two:
Filter captured packets by Port Source = 2021 or IP destination = 0.0.0.0 or protocol type = ARP OR length = 40.
Command:  portsource=2021 | ipdest=0.0.0.0 | prot=ARP | length=40

Before all commands you must enter "ENTER" key;
