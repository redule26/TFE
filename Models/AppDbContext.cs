using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VWA_TFE.Models
{
    public partial class AppDbContext : IdentityDbContext<AppUser> //On tient compte de AppUser pour créer la database en lien avec Identity
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) //transmet un paramètre à la classe de base
        {
        }
    }
}
