# netsnake
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
