using Ecom.Application.Common.Configuration;
using Ecom.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Ecom.IntegrationTests.Auth;
public sealed class BcryptPasswordHasherTests
{
 [Fact] public void Hash_verify_and_rehash_policy_are_consistent()
 {
  var hasher=new BcryptPasswordHasher(Options.Create(new PasswordSettings{BcryptWorkFactor=10,MinLength=15,MaxLength=128}));
  var hash=hasher.HashPassword("a sufficiently long passphrase");
  Assert.True(hasher.VerifyPassword("a sufficiently long passphrase",hash));
  Assert.False(hasher.VerifyPassword("wrong password",hash));
  Assert.False(hasher.NeedsRehash(hash));
 }

 [Fact] public void Lower_cost_hash_requires_rehash()
 {
  var oldHasher=new BcryptPasswordHasher(Options.Create(new PasswordSettings{BcryptWorkFactor=10,MinLength=15,MaxLength=128}));
  var currentHasher=new BcryptPasswordHasher(Options.Create(new PasswordSettings{BcryptWorkFactor=12,MinLength=15,MaxLength=128}));
  Assert.True(currentHasher.NeedsRehash(oldHasher.HashPassword("a sufficiently long passphrase")));
 }
}
