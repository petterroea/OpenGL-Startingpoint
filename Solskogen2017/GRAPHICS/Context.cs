using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Solskogen2017.EFFECTS;

namespace Solskogen2017.GRAPHICS
{
    class Context
    {
        private Vector3 _lookAt = new Vector3(0f, 0f, 0f);
        private Vector3 _cameraPosition = new Vector3(10f, 10f, 10f);
        private Vector3 _cameraUp = new Vector3(0f, 1f, 0f);
        private Vector3 _clearColor = new Vector3(0f, 0f, 0f);

        private Matrix4 _view = new Matrix4();
        private Matrix4 _projection = new Matrix4();

        //State thingies
        private MatrixMode _currentMatrixMode = MatrixMode.CAMERA;

        private Vector3 _lightPos = new Vector3(100f, 100f, 100f);
        private Vector3 _lightLookat = new Vector3(0f, 0f, 0f);
        private Matrix4 _lightView = new Matrix4();
        private Matrix4 _lightProjection = new Matrix4();

        private ShadowEffect _shadow;

        public enum MatrixMode
        {
            CAMERA,
            LIGHT
        }

        public Context(float fov, float aspect, float near, float far, ShadowEffect effect)
        {
            this._shadow = effect;
            _projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, 1f, 300f);
            //_projection = Matrix4.CreateOrthographic(100f, 100f, 1f, 100f);
            _lightProjection = Matrix4.CreateOrthographic(100f, 100f, 1f, 300f); //Not a point light
            //_lightProjection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, 1f, 70f);
        }

        public ShadowEffect ShadowEffect
        {
            get { return _shadow; }
        }

        public Vector3 LookAt
        {
            get { return _lookAt; }
            set { _lookAt = value; reloadViewMatrix(); }
        }
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set { _cameraPosition = value; reloadViewMatrix(); }
        }
        public Vector3 CameraUp
        {
            get { return _cameraUp; }
            set { _cameraUp = value; reloadViewMatrix(); }
        }
        public Vector3 LightPos
        {
            get { return _lightPos; }
            set {
                _lightPos = value;
                reloadLightView();
            }
        }
        public Vector3 LightLookAt
        {
            get { return _lightLookat; }
            set
            {
                _lightLookat = value;
                reloadLightView();
            }
        }

        public Matrix4 CurrentProjection
        {
            get
            {
                switch(_currentMatrixMode)
                {
                    case MatrixMode.CAMERA:
                        return _projection;
                    case MatrixMode.LIGHT:
                        return _lightProjection;
                }
                return Matrix4.Identity;
            }
        }

        public Matrix4 CurrentView
        {
            get
            {
                switch (_currentMatrixMode)
                {
                    case MatrixMode.CAMERA:
                        return _view;
                    case MatrixMode.LIGHT:
                        return _lightView;
                }
                return Matrix4.Identity;
            }
        }

        public Matrix4 CameraProjection
        {
            get { return _projection; }
        }

        public Matrix4 CameraView
        {
            get { return _view; }
        }

        public Matrix4 LightView
        {
            get
            {
                return _lightView;
            }
        }

        public Matrix4 LightProjection
        {
            get
            {
                return _lightProjection;
            }
        }

        public MatrixMode CurrentMode
        {
            get { return _currentMatrixMode; }
            set { _currentMatrixMode = value; }
        }

        private void reloadViewMatrix()
        {
            _view = Matrix4.LookAt(_cameraPosition, _lookAt, _cameraUp);
        }
        private void reloadLightView()
        {
            _lightView = Matrix4.LookAt(_lightPos, _lightLookat, _cameraUp);
            //_lightView = Matrix4.LookAt(_cameraPosition, _lookAt, _cameraUp);
        }
    }
}
