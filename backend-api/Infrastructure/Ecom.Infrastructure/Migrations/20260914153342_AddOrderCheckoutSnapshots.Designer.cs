using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260914153342_AddOrderCheckoutSnapshots")]
partial class AddOrderCheckoutSnapshots
{
}
