using DocumentManagementBackend.Domain.Enums;
using DocumentManagementBackend.Domain.Exceptions;
using DocumentManagementBackend.Domain.ValueObjects;
using NUnit.Framework;
using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Domain.UnitTests.Entities;

public class UserTests
{
    public class Create
    {
        [Test]
        public void Should_Create_User_With_Default_Active_Status()
        {
            // Act
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "hashedpassword",
                UserRole.User);

            // Assert
            Assert.That(user.Status, Is.EqualTo(UserStatus.Active));
        }

        [Test]
        public void Should_Set_Initial_Role()
        {
            // Act
            var user = User.Create(
                Email.Create("admin@example.com"),
                "Admin",
                "User",
                "hashedpassword",
                UserRole.Admin);

            // Assert
            Assert.That(user.Roles, Contains.Item(UserRole.Admin));
            Assert.That(user.HasRole(UserRole.Admin), Is.True);
        }

        [Test]
        public void Should_Throw_When_FirstName_IsEmpty()
        {
            Assert.Throws<DomainException>(() =>
                User.Create(
                    Email.Create("test@example.com"),
                    "",
                    "Doe",
                    "hashedpassword"));
        }

        [Test]
        public void Should_Throw_When_PasswordHash_IsEmpty()
        {
            Assert.Throws<DomainException>(() =>
                User.Create(
                    Email.Create("test@example.com"),
                    "John",
                    "Doe",
                    ""));
        }
    }

    public class Lock
    {
        [Test]
        public void Should_Lock_Active_User()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "hashedpassword");

            // Act
            user.Lock("Suspicious activity");

            // Assert
            Assert.That(user.Status, Is.EqualTo(UserStatus.Locked));
        }

        [Test]
        public void Should_Throw_When_Already_Locked()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "hashedpassword");
            user.Lock("Reason");

            // Act & Assert
            Assert.Throws<DomainException>(() => user.Lock("Another reason"));
        }
    }

    public class Unlock
    {
        [Test]
        public void Should_Unlock_Locked_User()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "hashedpassword");
            user.Lock("Reason");

            // Act
            user.Unlock();

            // Assert
            Assert.That(user.Status, Is.EqualTo(UserStatus.Active));
        }

        [Test]
        public void Should_Throw_When_Not_Locked()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "hashedpassword");

            // Act & Assert
            Assert.Throws<DomainException>(() => user.Unlock());
        }
    }

    public class ChangePassword
    {
        [Test]
        public void Should_Change_Password_When_Active()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "oldpassword");

            // Act
            user.ChangePassword("newpassword");

            // Assert
            Assert.That(user.PasswordHash, Is.EqualTo("newpassword"));
        }

        [Test]
        public void Should_Throw_When_User_Is_Locked()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password");
            user.Lock("Reason");

            // Act & Assert
            Assert.Throws<UserLockedException>(() => user.ChangePassword("newpassword"));
        }
    }

    public class AddRole
    {
        [Test]
        public void Should_Add_New_Role()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password",
                UserRole.User);

            // Act
            user.AddRole(UserRole.Manager);

            // Assert
            Assert.That(user.Roles, Has.Count.EqualTo(2));
            Assert.That(user.HasRole(UserRole.Manager), Is.True);
        }

        [Test]
        public void Should_Throw_When_Role_Already_Exists()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password",
                UserRole.User);

            // Act & Assert
            Assert.Throws<DomainException>(() => user.AddRole(UserRole.User));
        }
    }

    public class RemoveRole
    {
        [Test]
        public void Should_Remove_Existing_Role()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password",
                UserRole.User);
            user.AddRole(UserRole.Manager);

            // Act
            user.RemoveRole(UserRole.Manager);

            // Assert
            Assert.That(user.Roles, Has.Count.EqualTo(1));
            Assert.That(user.HasRole(UserRole.Manager), Is.False);
        }

        [Test]
        public void Should_Throw_When_Removing_Last_Role()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password",
                UserRole.User);

            // Act & Assert
            Assert.Throws<DomainException>(() => user.RemoveRole(UserRole.User));
        }

        [Test]
        public void Should_Throw_When_Role_Not_Present()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password",
                UserRole.User);

            // Act & Assert
            Assert.Throws<DomainException>(() => user.RemoveRole(UserRole.Admin));
        }
    }

    public class CanLogin
    {
        [Test]
        public void Should_Return_True_When_Active()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password");

            // Assert
            Assert.That(user.CanLogin(), Is.True);
        }

        [Test]
        public void Should_Return_False_When_Locked()
        {
            // Arrange
            var user = User.Create(
                Email.Create("test@example.com"),
                "John",
                "Doe",
                "password");
            user.Lock("Reason");

            // Assert
            Assert.That(user.CanLogin(), Is.False);
        }
    }
}