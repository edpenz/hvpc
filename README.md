# Hyper-V SSH Proxy Command

Helper for SSHing to Hyper-V virtual machines from Windows.
* Automatically boots VM if it's not running
* Automatically resolves VM IP

Usage:
```
ssh -oProxyCommand="hvpc.exe %h %p" ExampleVM
```
Or add to `%USERPROFILE%\.ssh\config`:
```
Host ExampleVM
  # SSH host name should be the Hyper-V VM name instead of network hostname
  HostName ExampleVM

  # Delegate connection establishment to HVPC
  ProxyCommand hvpc.exe %h %p
```
