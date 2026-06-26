using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Constants;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.Repository.Models;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class StaffServiceTests
{
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<IMapper> _mockMapper;
    private StaffService _staffService;

    [SetUp]
    public void Setup()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _staffService = new StaffService(_mockUserRepo.Object, _mockMapper.Object);
    }

    [Test]
    public async Task GetStaffAsync_ShouldReturnMappedStaff()
    {
        var staffList = new List<User> { new User { Id = 1 } };
        _mockUserRepo.Setup(r => r.GetActiveStaffAsync(false)).ReturnsAsync(staffList);
        _mockMapper.Setup(m => m.Map<IEnumerable<StaffResponseDTO>>(staffList))
            .Returns(new List<StaffResponseDTO> { new StaffResponseDTO { Id = 1 } });

        var result = await _staffService.GetStaffAsync(1, 10, false);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task GetStaffAsync_ShouldSort()
    {
        var staffList = new List<User>
        {
            new User { Id = 1, Email = "a@test.com" },
            new User { Id = 2, Email = "b@test.com" }
        };
        _mockUserRepo.Setup(r => r.GetActiveStaffAsync(false)).ReturnsAsync(staffList);

        _mockMapper.Setup(m => m.Map<IEnumerable<StaffResponseDTO>>(It.IsAny<IEnumerable<User>>()))
            .Returns((IEnumerable<User> src) => src.Select(u => new StaffResponseDTO { Id = u.Id, Email = u.Email }));

        var result = await _staffService.GetStaffAsync(1, 10, false, sortBy: "Email", sortDescending: true);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(2));
        Assert.That(data[0].Email, Is.EqualTo("b@test.com"));
        Assert.That(data[1].Email, Is.EqualTo("a@test.com"));
    }

    [Test]
    public void CreateStaffAsync_ShouldThrow_IfInvalidRole()
    {
        var dto = new StaffRegisterRequestDTO { Role = "InvalidRole" };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.CreateStaffAsync(dto));
        Assert.That(ex.Message, Does.Contain("Invalid role"));
    }

    [Test]
    public void CreateStaffAsync_ShouldThrow_IfEmailExists()
    {
        var dto = new StaffRegisterRequestDTO { Role = UserRoles.FrontDesk, Email = "test@test.com" };
        _mockUserRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(new User { Id = 1 });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _staffService.CreateStaffAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task CreateStaffAsync_ShouldAddAndReturn()
    {
        var dto = new StaffRegisterRequestDTO
        {
            Email = "staff@test.com",
            Password = "Password123!",
            FirstName = "Staff",
            LastName = "Member",
            Role = UserRoles.FrontDesk
        };

        _mockUserRepo.Setup(r => r.GetByEmailAsync("staff@test.com")).ReturnsAsync((User?)null);
        _mockMapper.Setup(m => m.Map<StaffResponseDTO>(It.IsAny<User>()))
            .Returns(new StaffResponseDTO { Id = 1, Email = "staff@test.com" });

        var result = await _staffService.CreateStaffAsync(dto);

        Assert.That(result.Email, Is.EqualTo("staff@test.com"));
        _mockUserRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "staff@test.com" && u.IsActive && u.Role == UserRoles.FrontDesk)), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateStaffAsync_ShouldThrow_IfNotFoundOrInactive()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.UpdateStaffAsync(99, new UpdateStaffDTO()));
        Assert.That(ex.Message, Does.Contain("Active staff member not found"));

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, IsActive = false });
        var ex2 = Assert.ThrowsAsync<ArgumentException>(() => _staffService.UpdateStaffAsync(1, new UpdateStaffDTO()));
        Assert.That(ex2.Message, Does.Contain("Active staff member not found"));
    }

    [Test]
    public void UpdateStaffAsync_ShouldThrow_IfCustomerAccount()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, IsActive = true, Role = "RegisteredUser" });
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.UpdateStaffAsync(1, new UpdateStaffDTO()));
        Assert.That(ex.Message, Does.Contain("Cannot modify a customer account"));
    }

    [Test]
    public void UpdateStaffAsync_ShouldThrow_IfInvalidRole()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, IsActive = true, Role = UserRoles.FrontDesk });
        var dto = new UpdateStaffDTO { Role = "InvalidRole" };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.UpdateStaffAsync(1, dto));
        Assert.That(ex.Message, Does.Contain("Invalid role"));
    }

    [Test]
    public async Task UpdateStaffAsync_ShouldUpdateAndReturn()
    {
        var staff = new User { Id = 1, IsActive = true, Role = UserRoles.FrontDesk, FirstName = "Old" };
        var dto = new UpdateStaffDTO { Role = UserRoles.Admin, FirstName = "New", LastName = "Name" };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _mockMapper.Setup(m => m.Map<StaffResponseDTO>(staff)).Returns(new StaffResponseDTO { Id = 1, FirstName = "New" });

        var result = await _staffService.UpdateStaffAsync(1, dto);

        Assert.That(staff.FirstName, Is.EqualTo("New"));
        Assert.That(staff.Role, Is.EqualTo(UserRoles.Admin));

        _mockUserRepo.Verify(r => r.Update(staff), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteStaffAsync_ShouldThrow_IfNotFound()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.DeleteStaffAsync(99));
        Assert.That(ex.Message, Does.Contain("Staff member not found"));
    }

    [Test]
    public void DeleteStaffAsync_ShouldThrow_IfCustomerAccount()
    {
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, Role = "RegisteredUser" });
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _staffService.DeleteStaffAsync(1));
        Assert.That(ex.Message, Does.Contain("Cannot delete a customer account"));
    }

    [Test]
    public async Task DeleteStaffAsync_ShouldSoftDelete()
    {
        var staff = new User { Id = 1, Role = UserRoles.FrontDesk, IsActive = true };
        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);

        await _staffService.DeleteStaffAsync(1);

        Assert.That(staff.IsActive, Is.False);
        _mockUserRepo.Verify(r => r.Update(staff), Times.Once);
        _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateStaffAsync_WithIsActiveValue_ShouldUpdateIsActive()
    {
        var staff = new User { Id = 1, IsActive = true, Role = UserRoles.FrontDesk };
        var dto = new UpdateStaffDTO { Role = UserRoles.Admin, IsActive = false };

        _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _mockMapper.Setup(m => m.Map<StaffResponseDTO>(staff)).Returns(new StaffResponseDTO { Id = 1 });

        await _staffService.UpdateStaffAsync(1, dto);

        Assert.That(staff.IsActive, Is.False);
    }
}
