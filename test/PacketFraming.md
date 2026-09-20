# Packet framing regression tests (1.4.4)

After the normal tModLoader contributor setup, run from the repository root:

```sh
dotnet test test/tModLoaderTests.csproj --filter FullyQualifiedName~PacketFramingTests
```

The tests use the existing test project's reference to the checkout's Terraria
project.

`NetMessage.CheckBytes` and connection termination run unchanged. A scoped
MonoMod hook records payload dispatch instead of running `MessageBuffer.GetData`,
so these tests require no world, content loading, graphics, or sockets. The hook
bounds dispatch attempts so the original zero-length loop fails rather than
hanging. It is disposed after each test, and replaced connection/buffer state
and network-mode flags are restored. The fixture is not parallelized.
